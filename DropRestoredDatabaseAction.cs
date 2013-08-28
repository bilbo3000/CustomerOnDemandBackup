namespace Microsoft.Crm.CrmLive.Provisioning
{
	using System;
	using Microsoft.Crm.Config.Wrapper;
	using Microsoft.Crm.Setup.Server.Utility;
	using System.Data.SqlClient; 

	internal sealed class DropRestoredDatabaseAction : OnDemandBackupActionBase
	{
		public override OrganizationBackupStatus Step { get { return OrganizationBackupStatus.AvailableOnShare; } }

		protected override OrganizationBackupRuntimeData ExecuteInternal(OrganizationBackupRuntimeData runtimeData, Guid orgId, Guid backupId, Guid queueItemId)
		{
			Exceptions.ThrowIfNullOrEmpty(runtimeData.RestoreSqlServerName, "runtimeData.RestoreSqlServerName");
			Exceptions.ThrowIfNullOrEmpty(runtimeData.RestoredDBName, "runtimeData.RestoredDBName");

			try
			{
				// drop restored db
				SqlUtility.DropDatabase(runtimeData.RestoreSqlServerName, runtimeData.RestoredDBName);
			}
			catch (SqlException ex)
			{
				// error 3701: database not found. If database is not found, we don't need to do anything. 
				// Re-throw the exception for other type of exceptions, so executor can retry
				if (ex.Number != 3701)  
				{
					throw; 
				}
			}

			// update restore sql server name and restored db name 
			runtimeData.RestoreSqlServerName = null;  // clear restore sql server name 
			runtimeData.RestoredDBName = null;  // clear restore db name
			UpdateRuntimeData(queueItemId, runtimeData);
			
			return runtimeData;
		}
	}
}
