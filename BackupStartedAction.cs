namespace Microsoft.Crm.CrmLive.Provisioning
{
	using System;
	using Microsoft.Crm.Config.Wrapper;
	using Microsoft.Crm.ConfigurationDatabase;
	using Microsoft.Crm.SharedDatabase;

	internal sealed class  BackupStartedAction : OnDemandBackupActionBase 
	{

		public override OrganizationBackupStatus Step { get { return OrganizationBackupStatus.BackupStarted;  } }
		
		protected override OrganizationBackupRuntimeData ExecuteInternal(OrganizationBackupRuntimeData runtimeData, Guid orgId, Guid backupId, Guid queueItemId)
		{
			Guid orgScaleGroupId = LocatorService.Instance.GetOrganizationScaleGroupId(orgId); 
			
			// full backup
			CrmOrganizationBackupData backupData = SqlBackupRestoreUtility.ExecuteBackupSqlCommand(orgId, orgScaleGroupId);

			// update backup path and log path in runtime data
			runtimeData.BackupPath = backupData.BackupPathOnShare;
			runtimeData.LogPath = backupData.LogPathOnShare; 
			QueueManager.NewManager().UpdateQueueItemRuntimeData(queueItemId, OrganizationBackupRuntimeData.Serialize(runtimeData)); 
			return runtimeData;
		}
	}
}
