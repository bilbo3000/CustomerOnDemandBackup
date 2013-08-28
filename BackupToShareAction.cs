namespace Microsoft.Crm.CrmLive.Provisioning
{
	using System;
	using Microsoft.Crm.Config.Wrapper;
	using Microsoft.Crm.ConfigurationDatabase;
	using Microsoft.Crm.SharedDatabase;
	using Microsoft.Crm.Data;

	internal sealed class BackupToShareAction : OnDemandBackupActionBase
	{
		public override OrganizationBackupStatus Step { get { return OrganizationBackupStatus.BackupToShare; } }

		protected override OrganizationBackupRuntimeData ExecuteInternal(OrganizationBackupRuntimeData runtimeData, Guid orgId, Guid backupId, Guid queueItemId)
		{
			Exceptions.ThrowIfNullOrEmpty(runtimeData.RestoreSqlServerName, "runtimeData.RestoreSqlServerName");
			Exceptions.ThrowIfNullOrEmpty(runtimeData.RestoredDBName, "runtimeData.RestoredDBName");

			string orgUniqueName = LocatorService.Instance.GetOrganizationName(orgId);

			// Get the path to the file share from ServerSettingProperties table DataFilePath row
			string fileSharePath = (string)LocatorService.Instance.GetSiteSetting("DataFilePath");

			// compression backup
			CrmOrganizationBackupData backupData = SqlBackupRestoreUtility.ExecuteBackupSqlCommandByOrg(SqlBackupRestoreUtility.DatabaseBackupType.Compression, fileSharePath, runtimeData.RestoreSqlServerName, runtimeData.RestoredDBName, orgUniqueName);

			// update backup path, completion time and expiration time 
			PropertyBag updateBag = new PropertyBag();
			updateBag["BackupPath"] = backupData.BackupPathOnShare;
			updateBag["CompletionDate"] = DateTime.UtcNow;
			updateBag["ExpirationDate"] = ((DateTime)updateBag["CompletionDate"]).AddDays(GetExpirationDays(orgId));

			PropertyBag condition = new PropertyBag();
			condition["Id"] = backupId;
			using (IDatabaseService service = ConfigurationDatabaseService.NewService())
			{
				service.Update("OrganizationBackupData", updateBag, new PropertyBag[] { condition });
			}

			return runtimeData;
		}

		private static int GetExpirationDays(Guid orgId)
		{
			int expirationDays = 0;
			var temp = LocatorService.Instance.GetOrganizationSetting(orgId, "OnDemandBackupExpirationDays");
			if (temp != null)
			{
				expirationDays = (int)temp;
			}
			else
			{
				expirationDays = (int)LocatorService.Instance.GetSiteSetting("OnDemandBackupExpirationDays");
			}

			return Math.Max(0, expirationDays); 
		}
	}
}
