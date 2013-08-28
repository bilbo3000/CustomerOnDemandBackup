namespace Microsoft.Crm.CrmLive.Provisioning
{
	using System;
	using System.IO;
	using Microsoft.Crm.Config.Wrapper;

	internal sealed class DeleteTemporaryBackupAction : OnDemandBackupActionBase
	{
		public override OrganizationBackupStatus Step { get { return OrganizationBackupStatus.DeleteOldBackup; } }

		protected override OrganizationBackupRuntimeData ExecuteInternal(OrganizationBackupRuntimeData runtimeData, Guid orgId, Guid backupId, Guid queueItemId)
		{
			// delete initial full backup file and log file 
			DeleteIfNotNull(runtimeData.BackupPath);
			DeleteIfNotNull(runtimeData.LogPath);

			// update backup path and log path in runtime data
			runtimeData.BackupPath = null;  // clear backup path 
			runtimeData.LogPath = null;  // clear log path 
			UpdateRuntimeData(queueItemId, runtimeData);

			return runtimeData;
		}

		private static void DeleteIfNotNull(string path)
		{
			if (!String.IsNullOrWhiteSpace(path))
			{
				File.Delete(path);
			}
		}

	}

}
