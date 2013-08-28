namespace Microsoft.Crm.CrmLive.Provisioning
{
	using System;
	using Microsoft.Crm.Config.Wrapper;
	using Microsoft.Crm.ConfigurationDatabase;
	using Microsoft.Crm.Data;
	using Microsoft.Crm.SharedDatabase;

	internal abstract class OnDemandBackupActionBase
	{
		public abstract OrganizationBackupStatus Step { get; }

		public OrganizationBackupRuntimeData Execute(OrganizationBackupRuntimeData runtimeData, Guid orgId, Guid backupId, Guid queueItemId)
		{
			Exceptions.ThrowIfNull(runtimeData, "runtimeData");
			Exceptions.ThrowIfGuidEmpty(orgId, "orgId");
			Exceptions.ThrowIfGuidEmpty(backupId, "backupId");
			Exceptions.ThrowIfGuidEmpty(queueItemId, "queueItemId");

			// update status code in the table 
			UpdateStatus(backupId, Step);

			return ExecuteInternal(runtimeData, orgId, backupId, queueItemId);
		}

		protected abstract OrganizationBackupRuntimeData ExecuteInternal(OrganizationBackupRuntimeData runtimeData, Guid orgId, Guid backupId, Guid queueItemId);

		private static void UpdateStatus(Guid backupId, OrganizationBackupStatus statusCode)
		{
			// update status code in the table 
			PropertyBag updateBag = new PropertyBag();
			updateBag["StatusCode"] = (int)statusCode;
			PropertyBag condition = new PropertyBag();
			condition["Id"] = backupId; 
			using (IDatabaseService service = ConfigurationDatabaseService.NewService())
			{
				service.Update("OrganizationBackupData", updateBag, new PropertyBag[] { condition });
			}
		}

		protected static void UpdateRuntimeData(Guid queueItemId, OrganizationBackupRuntimeData runtimeData)
		{
			QueueManager.NewManager().UpdateQueueItemRuntimeData(queueItemId, OrganizationBackupRuntimeData.Serialize(runtimeData));
		}
	}
}
