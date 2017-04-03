using System.Collections.Generic;
using System.Security;
using mRemoteNG.Config.DatabaseConnectors;
using mRemoteNG.Config.DataProviders;
using mRemoteNG.Config.Putty;
using mRemoteNG.Config.Serializers;
using mRemoteNG.Credential;
using mRemoteNG.Tools;
using mRemoteNG.Tree;
using mRemoteNG.UI.Forms;


namespace mRemoteNG.Config.Connections
{
	public class ConnectionsLoader
	{		
        public bool UseDatabase { get; set; }
	    public string ConnectionFileName { get; set; }
		

		public ConnectionTreeModel LoadConnections(IEnumerable<ICredentialRecord> credentialRecords, bool import)
		{
		    ConnectionTreeModel connectionTreeModel;

            if (UseDatabase)
			{
			    var connector = new SqlDatabaseConnector();
			    var dataProvider = new SqlDataProvider(connector);
                var databaseVersionVerifier = new SqlDatabaseVersionVerifier(connector);
			    databaseVersionVerifier.VerifyDatabaseVersion();
                var dataTable = dataProvider.Load();
			    var deserializer = new DataTableDeserializer();
                connectionTreeModel = deserializer.Deserialize(dataTable);
            }
			else
			{
			    var dataProvider = new FileDataProvider(ConnectionFileName);
			    var xmlString = dataProvider.Load();
			    var deserializer = new XmlConnectionsDeserializer(credentialRecords, PromptForPassword);
                connectionTreeModel = deserializer.Deserialize(xmlString);
            }

            if (connectionTreeModel != null)
			    FrmMain.Default.ConnectionsFileName = ConnectionFileName;
            else
                connectionTreeModel = new ConnectionTreeModel();

		    if (import) return connectionTreeModel;
		    PuttySessionsManager.Instance.AddSessions();
            connectionTreeModel.RootNodes.AddRange(PuttySessionsManager.Instance.RootPuttySessionsNodes);

		    return connectionTreeModel;
		}

        private SecureString PromptForPassword()
        {
            var password = MiscTools.PasswordDialog("", false);
            return password;
        }
    }
}