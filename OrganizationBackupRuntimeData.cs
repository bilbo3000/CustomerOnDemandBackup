namespace Microsoft.Crm.CrmLive.Provisioning
{
	using System.Xml.Serialization;
	using Microsoft.Crm.CrmLive; 

	[XmlRoot("OrganizationBackupRuntimeData")]
	public sealed class OrganizationBackupRuntimeData
	{
		#region public properties
		public string BackupPath { get; set; }

		public string LogPath { get; set; }

		public string RestoredDBName { get; set; }

		public string RestoreSqlServerName { get; set; }
		#endregion

		#region Serialization/Deserialization methods
		public static OrganizationBackupRuntimeData Deserialize(string runtimeDataXml)
		{
			return CrmLiveSerializer.Xml2Object<OrganizationBackupRuntimeData>(runtimeDataXml); 
		}

		public static string Serialize(OrganizationBackupRuntimeData runtimeData)
		{
			return CrmLiveSerializer.Object2Xml<OrganizationBackupRuntimeData>(runtimeData, false); 
		}
		#endregion 
	}
}
