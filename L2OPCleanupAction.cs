namespace Microsoft.Crm.CrmLive.Provisioning
{
	using System;
	using System.Data.SqlClient;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Reflection;
	using Microsoft.Crm;
	using Microsoft.Crm.Config.Wrapper;
	using Microsoft.Crm.Metadata;

	internal sealed class L2OPCleanupAction : OnDemandBackupActionBase
	{
		public override OrganizationBackupStatus Step { get { return OrganizationBackupStatus.L2OPCleanup; } }

		protected override OrganizationBackupRuntimeData ExecuteInternal(OrganizationBackupRuntimeData runtimeData, Guid orgId, Guid backupId, Guid queueItemId)
		{
			Exceptions.ThrowIfNullOrEmpty(runtimeData.RestoreSqlServerName, "runtimeData.RestoreSqlServerName");
			Exceptions.ThrowIfNullOrEmpty(runtimeData.RestoredDBName, "runtimeData.RestoredDBName");

			string sqlConnectionString = "Data Source=" + runtimeData.RestoreSqlServerName + ";Initial Catalog=" + runtimeData.RestoredDBName + ";Integrated Security=SSPI";
			using (SqlConnection connection = new SqlConnection(sqlConnectionString))
			{
				try
				{
					connection.Open();

					foreach (string fileName in L2OPScriptFileList.L2OPScripts)
					{
						ScriptExecutor.ExecuteScript(fileName, connection);
					}

					connection.Close();

					// Must clear the pool, otherwise get "database currently in use" error when try to drop db. 
					SqlConnection.ClearPool(connection);
				}
				catch (SqlException)
				{
					connection.Close();
					SqlConnection.ClearPool(connection);
					throw;
				}
				finally
				{
					connection.Close();
					SqlConnection.ClearPool(connection);
				}
			}

			return runtimeData;
		}
	}

	internal static class ScriptExecutor
	{
		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public static void ExecuteScript(string filename, SqlConnection connection)
		{
			string fullPath = Assembly.GetExecutingAssembly().Location;
			int index = fullPath.LastIndexOf(@"\", StringComparison.CurrentCulture);
			string currentDirectory = fullPath.Substring(0, index);
			string filePath = currentDirectory + @"\L2OPScripts\" + filename; 
			FileInfo file = new FileInfo(filePath);
			string script = file.OpenText().ReadToEnd();
			using (SqlCommand cmd = new SqlCommand())
			{
				cmd.Connection = connection;
				cmd.CommandText = script;
				cmd.CommandTimeout = 3600;
				SqlHelper.ExecuteBatches(null, cmd);
			}
		}
	}
}
