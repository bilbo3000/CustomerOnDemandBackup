namespace Microsoft.Crm.CrmLive.Provisioning
{
	using System;
	using Microsoft.Crm.Admin.AdminService;
	using Microsoft.Crm.Config.Wrapper;
	using Microsoft.Crm.ConfigurationDatabase;
	using Microsoft.Crm.SharedDatabase;

	internal sealed class RestoringToServerAction : OnDemandBackupActionBase
	{

		public override OrganizationBackupStatus Step { get { return OrganizationBackupStatus.RestoringToServer; } }

		protected override OrganizationBackupRuntimeData ExecuteInternal(OrganizationBackupRuntimeData runtimeData, Guid orgId, Guid backupId, Guid queueItemId)
		{
			Exceptions.ThrowIfNullOrEmpty(runtimeData.BackupPath, "runtimeData.BackupPath");

			string orgUniqueName = LocatorService.Instance.GetOrganizationName(orgId); 

			// retrieve restore sql server name and generate restored db name
			if (string.IsNullOrEmpty(runtimeData.RestoreSqlServerName) || string.IsNullOrEmpty(runtimeData.RestoredDBName))
			{
				runtimeData.RestoreSqlServerName = RetrieveRestoreSqlServer();
				runtimeData.RestoredDBName = orgUniqueName + "_" + Guid.NewGuid().ToString();
			}

			// update run time data with RestoreSqlServerName and RestoredDBName
			QueueManager.NewManager().UpdateQueueItemRuntimeData(queueItemId, OrganizationBackupRuntimeData.Serialize(runtimeData));

			// restore 
			SqlBackupRestoreUtility.ExecuteRestoreSqlCommand(runtimeData.RestoreSqlServerName, runtimeData.RestoredDBName, runtimeData.BackupPath, null);

			return runtimeData;
		}

		/// <summary>
		/// Select the server with the ServerRoles.BackupCleanupSqlServer role for restoring database. 
		/// </summary>
		private static string RetrieveRestoreSqlServer()
		{
			// Select the enabled sql server in the same datacenter with correct role. 
			ServerFilter serverFilter = new ServerFilter();
			serverFilter.Roles = ServerRoles.BackupCleanupSqlServer;
			serverFilter.State = ServerState.Enabled; 
			serverFilter.DatacenterId = LocatorService.Instance.GetDatacenterId();  // getting data from settings

			CrmServerService crmServerService = new CrmServerService();
			var res = crmServerService.RetrieveMultiple(serverFilter);
			if (res.Length == 0) 
			{
				throw new CrmException("cannot find BackupCleanupSqlServer has role: " + ServerRoles.BackupCleanupSqlServer);
			}

			// If multiple, randomly select one 
			// TODO: need to use queue item group id to control the number of allowed parallel queue items
			Random rand = new Random();
			int randNum = rand.Next(); 
			return res[randNum % res.Length].Name; 
		}
	}
}
