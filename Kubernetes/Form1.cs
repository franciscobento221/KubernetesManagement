using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Newtonsoft.Json;
using System.Collections.Generic;
using KubeClient.Models;
using ComboBox = System.Windows.Forms.ComboBox;
using static Kubernetes.Form1;
using static KubeClient.K8sAnnotations;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Speech.Recognition;
using System.Globalization;
using YamlDotNet.Core.Tokens;

namespace Kubernetes
{
    public partial class Form1 : Form
    {
       
        private MemoryInfo memoryInfo;

        private Timer timer1;
        private Timer timer2;
        SpeechRecognitionEngine recognizer;



        public Form1()
        {

            InitializeComponent();
            tabControl1.Hide();
            listViewNodes.Hide();
            LabelDashboard.Hide();
            textBoxToken.ReadOnly = true;
            checkBoxToken.CheckedChanged += CheckBoxToken_CheckedChanged;
            memoryInfo = new MemoryInfo();
            ServicePointManager.ServerCertificateValidationCallback +=
            (sender, cert, chain, sslPolicyErrors) => true;

            buttonVoiceNamespace.MouseDown += ButtonVoiceNamespace_MouseDown;
            buttonVoiceNamespace.MouseUp += ButtonVoiceNamespace_MouseUp;
            buttonVoiceNamespace.MouseLeave += ButtonVoiceNamespace_MouseLeave;
        }

        private void CheckBoxToken_CheckedChanged(object sender, EventArgs e)
        {
            // Set TextBox.ReadOnly based on CheckBox.Checked
            textBoxToken.ReadOnly = !checkBoxToken.Checked;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            comboBoxImagemPOD.Items.Add("nginx");
            comboBoxImagemPOD.Items.Add("httpd");
            comboBoxImagemPOD.Items.Add("node");

            comboBoxImageDeployment.Items.Add("nginx");
            comboBoxImageDeployment.Items.Add("httpd");
            comboBoxImageDeployment.Items.Add("node");

            comboBoxRestartPolicyPod.Items.Add("Always");
            comboBoxRestartPolicyPod.Items.Add("OnFailure");
            comboBoxRestartPolicyPod.Items.Add("Never");

            comboBoxProtocoloService.Items.Add("TCP");
            comboBoxProtocoloService.Items.Add("UDP");
            comboBoxProtocoloService.Items.Add("SCTP");

            comboBoxServiceTypeService.Items.Add("ClusterIP");
            comboBoxServiceTypeService.Items.Add("NodePort");
            comboBoxServiceTypeService.Items.Add("LoadBalancer");


            if (checkBoxToken.Checked == true)
            {
                textBoxToken.ReadOnly = false;
            }
         




        }

        #region Classes
        public class Metadata
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("namespace")]
            public string Namespace { get; set; }

        }
        public class UsageNode
        {
            public string Cpu { get; set; }
            public string Memory { get; set; }
        }

        public class Node
        {
            public Metadata metadata { get; set; }
            public UsageNode Usage { get; set; }
        }
        public class NodeResponse
        {
            public List<Node> items { get; set; }
        }

        public class Namespace
        {
            public Metadata metadata { get; set; }
        }
        public class NamespaceResponse
        {
            public List<Namespace> items { get; set; }
        }

        public class Pods
        {
            public Metadata metadata { get; set; }
        }
        public class PodsResponse
        {
            public List<Pods> items { get; set; }
        }

        public class Deployment
        {
            public Metadata metadata { get; set; }
        }
        public class DeploymentsResponse
        {
            public List<Deployment> items { get; set; }
        }

        public class Services
        {
            public Metadata metadata { get; set; }
        }
        public class ServicesResponse
        {
            public List<Services> items { get; set; }
        }
        public class NodeMetrics
        {
            public List<NodeUsage> Items { get; set; }
        }

        public class NodeUsage
        {
            public Usage Usage { get; set; }
        }

        public class Usage
        {
            public string Cpu { get; set; }
            public string Memory { get; set; }
        }


        #endregion


        #region Login
        private void buttonConnect_Click(object sender, EventArgs e)
        {

            string endpointTeste = "";
            WebClient client = new WebClient();

            if (checkBoxToken.Checked)
            {
                endpointTeste = "https://" + textBoxIPPorta.Text + "/api";

                string bearerToken = ""+ textBoxToken.Text + "";  // Replace this with the actual way to get your token

                // Add the Authorization header with the bearer token
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpointTeste = "http://" + textBoxIPPorta.Text + "/api";// Assuming txtEndpoint is the name of your text box for the API endpoint
            }

            

            
            


            try
            {

                string response = client.DownloadString(endpointTeste);
                MessageBox.Show("API LIGADA !");
                tabControl1.Show();
                listViewNodes.Show();
                LabelDashboard.Show();

                //SetupListView();
                //DisplayNodeMetrics();


                timer1 = new Timer();
                timer1.Tick += new EventHandler(SetupListView);
                timer1.Interval = 2000; // in miliseconds
                timer1.Start();

                timer2 = new Timer();
                timer2.Tick += new EventHandler(DisplayNodeMetrics);
                timer2.Interval = 2000; // in miliseconds
                timer2.Start();


                textBoxIPPorta.ReadOnly = true;
                textBoxToken.ReadOnly = true;
                checkBoxToken.Enabled = false;


            }
            catch (Exception)
            {
                MessageBox.Show("API NÃO SE ENCONTRA LIGADA");
                
            }
            finally
            {
                client.Dispose();
            }



        }

        #endregion

        #region Nodes
       
        private void SetupListView(object sender, EventArgs e)
        {
            if (listViewNodes.Columns.Count == 0)
            {
                listViewNodes.View = View.Details;
                listViewNodes.Columns.Add("Node Name", -2, HorizontalAlignment.Left);
                listViewNodes.Columns.Add("CPU Usage (m)", -2, HorizontalAlignment.Left);
                listViewNodes.Columns.Add("Memory Usage (MiB)", -2, HorizontalAlignment.Left);
            }
        }

      

        private void DisplayNodeMetrics(object sender, EventArgs e)
        {
            listViewNodes.Items.Clear();
            WebClient client = new WebClient();
            string endpointTeste = "";

            if (checkBoxToken.Checked)
            {
                endpointTeste = "https://" + textBoxIPPorta.Text + "/apis/metrics.k8s.io/v1beta1/nodes";

                string bearerToken = "" + textBoxToken.Text + "";  // Replace this with the actual way to get your token

                // Add the Authorization header with the bearer token
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpointTeste = "http://" + textBoxIPPorta.Text + "/apis/metrics.k8s.io/v1beta1/nodes";// Assuming txtEndpoint is the name of your text box for the API endpoint
            }


            string response = client.DownloadString(endpointTeste);

            if (!string.IsNullOrEmpty(response))
            {
                var nodeMetrics = JsonConvert.DeserializeObject<NodeResponse>(response);
                foreach (var node in nodeMetrics.items)
                {

                    var listViewItem = new ListViewItem(node.metadata.Name);
                    listViewItem.SubItems.Add(ConvertNanocoresToMillicores(node.Usage.Cpu));
                    listViewItem.SubItems.Add(ConvertKibToMib(node.Usage.Memory));
                    listViewNodes.Items.Add(listViewItem);
                }
            }
        }

        private string ConvertNanocoresToMillicores(string nanocores)
        {
            // Converts nanocores to millicores for better readability
            long nCores = long.Parse(nanocores.Trim('n'));
            return (nCores / 1000000).ToString() + "m";
        }

        private string ConvertKibToMib(string kib)
        {
            // Converts KiB to MiB for better readability
            int kiB = int.Parse(kib.Trim('K', 'i'));
            return (kiB / 1024).ToString() + " MiB";
        }


        #endregion

        #region Namespace

        public void ListarNamespaces(Control control)
        {

            WebClient client = new WebClient();
            string endpoint = "";

            if (checkBoxToken.Checked)
            {
                endpoint = "https://" + textBoxIPPorta.Text + "/api/v1/namespaces";

                string bearerToken = "" + textBoxToken.Text + "";  // Replace this with the actual way to get your token

                // Add the Authorization header with the bearer token
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpoint = "http://" + textBoxIPPorta.Text + "/api/v1/namespaces";// Assuming txtEndpoint is the name of your text box for the API endpoint
            }


            string response = client.DownloadString(endpoint);

            try
            {
                var namespaceResponse = JsonConvert.DeserializeObject<NodeResponse>(response);

                // List of namespace names to exclude
                var excludedNamespaces = new HashSet<string> { "ingress", "kube-node-lease", "kube-public", "kube-system" };

                if (control is CheckedListBox checkedListBox)
                {
                    checkedListBox.Items.Clear();
                    foreach (var namespaceItem in namespaceResponse.items)
                    {
                        if (!excludedNamespaces.Contains(namespaceItem.metadata.Name))
                        {
                            checkedListBox.Items.Add(namespaceItem.metadata.Name);
                        }
                    }
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.Items.Clear();
                    foreach (var namespaceItem in namespaceResponse.items)
                    {
                        if (!excludedNamespaces.Contains(namespaceItem.metadata.Name))
                        {
                            comboBox.Items.Add(namespaceItem.metadata.Name);
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Control is not a supported type for listing items.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                client.Dispose();
            }
        }


        private void buttonListarNamespace_Click(object sender, EventArgs e)
        {

            ListarNamespaces(checkedListBox3);
        }

        private void buttonEliminarNamespace_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            string endpoint = "";

            

            foreach (var item in checkedListBox3.CheckedItems)
            {
                string namespaceName = item.ToString();
                if (CheckResourcesExist(namespaceName, "pods") || CheckResourcesExist(namespaceName, "services") || CheckResourcesExist(namespaceName, "deployments"))
                {
                    MessageBox.Show($"NÃO FOI POSSIVEL ELIMINAR O NAMESPACE '{namespaceName}', ELIMINE PRIMEIRO OS RECURSOS");
                    continue;
                }

                try
                {
                    if (checkBoxToken.Checked)
                    {
                        endpoint = $"https://" + textBoxIPPorta.Text + "/api/v1/namespaces/"+namespaceName+"";

                        string bearerToken = "" + textBoxToken.Text + "";

                        client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

                    }
                    else
                    {
                        endpoint = $"http://" + textBoxIPPorta.Text + "/api/v1/namespaces/"+namespaceName+"";
                    }
                    client.UploadString(endpoint, "DELETE", string.Empty);
                    MessageBox.Show("NAMESPACE ELIMINADO!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting namespace: " + ex.Message);
                }
            }
            client.Dispose();
        }
        private bool CheckResourcesExist(string namespaceName, string resourceType)
            {
            //string url = $"http://{textBoxIPPorta.Text}/api/v1/namespaces/{namespaceName}/{resourceType}";

            int token = 0;
            WebClient client = new WebClient();

            if (checkBoxToken.Checked)
            {
                
                token = 1;
                string bearerToken = "" + textBoxToken.Text + "";

                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;
            }
                
                
                using (client)
                {
                    try
                    {
                        
                        switch (resourceType)
                        {
                            case "pods":

                            string urlPod = "";

                            if (token == 1)
                            {
                                urlPod = $"https://{textBoxIPPorta.Text}/api/v1/namespaces/{namespaceName}/{resourceType}";
                            }
                            else
                            {
                                urlPod = $"http://{textBoxIPPorta.Text}/api/v1/namespaces/{namespaceName}/{resourceType}";
                            }

                                string responsePod = client.DownloadString(urlPod);
                                var podResponse = JsonConvert.DeserializeObject<PodsResponse>(responsePod);
                                return podResponse.items.Any();
                            case "services":
                            
                                string urlService = "";

                            if (token == 1)
                            {
                                urlService = $"https://{textBoxIPPorta.Text}/api/v1/namespaces/{namespaceName}/{resourceType}";
                            }
                            else
                            {
                                urlService = $"http://{textBoxIPPorta.Text}/api/v1/namespaces/{namespaceName}/{resourceType}";
                            }
                                string responseService = client.DownloadString(urlService);
                                var serviceResponse = JsonConvert.DeserializeObject<ServicesResponse>(responseService);
                                return serviceResponse.items.Any();
                            case "deployments":

                            string urlDeployment = "";

                            if (token == 1)
                            {
                                urlDeployment = $"https://{textBoxIPPorta.Text}/apis/apps/v1/namespaces/{namespaceName}/deployments";

                            }
                            else
                            {
                                urlDeployment = $"http://{textBoxIPPorta.Text}/apis/apps/v1/namespaces/{namespaceName}/deployments";
                            }

                            string responseDeploy = client.DownloadString(urlDeployment);
                            var deploymentResponse = JsonConvert.DeserializeObject<DeploymentsResponse>(responseDeploy); 
                                return deploymentResponse.items.Any();
                            default:
                                return false;
                        }
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine("Error fetching data: " + ex.Message);
                        return true; // Assume resources exist if the API call fails, to prevent accidental deletion.
                    }
                }
            }



        private void buttonCriarNamespace_Click(object sender, EventArgs e)
        {
            string endpoint = "";
            WebClient client = new WebClient();

            if (checkBoxToken.Checked)
            {
                endpoint = "https://" + textBoxIPPorta.Text + "/api/v1/namespaces";

                string bearerToken = "" + textBoxToken.Text + "";  // Replace this with the actual way to get your token

                // Add the Authorization header with the bearer token
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpoint = "http://" + textBoxIPPorta.Text + "/api/v1/namespaces";// Assuming txtEndpoint is the name of your text box for the API endpoint
            }


            string jsonContent = $@"{{
  ""apiVersion"": ""v1"",
  ""kind"": ""Namespace"",
  ""metadata"": {{
    ""name"": ""{textBoxNomeNamespace.Text}""
  }}
}}";





            string response = "";
            try
            {
                response = client.UploadString(endpoint, "POST", jsonContent);
                Console.WriteLine("Response: " + response);
                MessageBox.Show("NAMESPACE CRIADO !");
            }
            catch (WebException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                MessageBox.Show("ERRO AO CRIAR NAMESPACE!");
            }
            finally
            {

                client.Dispose();

            }
        }


        private void ButtonVoiceNamespace_MouseDown(object sender, MouseEventArgs e)
        {
            InitializeSpeechRecognition(); // Start recognition when the button is held down
        }

        private void ButtonVoiceNamespace_MouseUp(object sender, MouseEventArgs e)
        {
            StopSpeechRecognition(); // Stop recognition when the button is released
        }

        private void ButtonVoiceNamespace_MouseLeave(object sender, EventArgs e)
        {
            StopSpeechRecognition(); // Optionally stop recognition when the mouse leaves the button
        }

        private void InitializeSpeechRecognition()
        {
            if (recognizer == null)
            {
                recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US"));
                recognizer.SetInputToDefaultAudioDevice();

                GrammarBuilder gb = new GrammarBuilder();
                gb.Culture = new CultureInfo("en-US");

                var commands = new Choices();
                commands.Add("create namespace");
                commands.Add("list");
                commands.Add("delete");


                gb.Culture = new CultureInfo("en-US");
                gb.Append(commands);  // Add the choices to the grammar
                gb.AppendDictation();

                Grammar grammar = new Grammar(gb);
                recognizer.LoadGrammar(grammar);
                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            }
            Console.WriteLine("Speech recognition initialized and started listening...");
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void StopSpeechRecognition()
        {
            if (recognizer != null)
            {
                recognizer.RecognizeAsyncStop();
                Console.WriteLine("Speech recognition stopped.");
            }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string text = e.Result.Text;
            Console.WriteLine($"Recognized text: {text}");

            string commandPrefixCreate = "create namespace";
            string commandPrefixList = "list";
            string commandPrefixDelete = "delete";

            if (text.StartsWith(commandPrefixCreate))
            {
                string namespaceName = text.Substring(commandPrefixCreate.Length).Trim();
                textBoxNomeNamespace.Text = namespaceName;
                Console.WriteLine($"Extracted namespace name: {namespaceName}");

                buttonCriarNamespace_Click(this, EventArgs.Empty);
            }
            else if(text.StartsWith(commandPrefixList))
            {
                buttonListarNamespace_Click(this, EventArgs.Empty);
            }
            else if (text.StartsWith(commandPrefixDelete))
            {
                string namespaceToDeleted = text.Substring(commandPrefixDelete.Length).Trim();

                buttonListarNamespace_Click(this, EventArgs.Empty);


                for (int i = 0; i < checkedListBox3.Items.Count; i++)
                {
                    if (checkedListBox3.Items[i].ToString() == namespaceToDeleted)
                    {
                        checkedListBox3.SetItemChecked(i, true); // This checks the checkbox
                        checkedListBox3.SelectedIndex = i; // This highlights the item in the UI
                        Console.WriteLine($"Namespace '{namespaceToDeleted}' found and selected.");
                        buttonEliminarNamespace_Click(this, EventArgs.Empty);
                        return;
                    }
                }


                //Console.WriteLine($"Extracted namespace name: {namespaceName}");

                
            }
            else
            {
                Console.WriteLine("Command not recognized or not matching the expected format.");
            }
        }


        #endregion

        #region Pods


        private void buttonListarPod_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();     
            string endpoint = "";

            if (checkBoxToken.Checked)
            {
                endpoint = "https://" + textBoxIPPorta.Text + "/api/v1/pods";

                string bearerToken = "" + textBoxToken.Text + "";  // Replace this with the actual way to get your token

                // Add the Authorization header with the bearer token
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpoint = "http://" + textBoxIPPorta.Text + "/api/v1/pods";// Assuming txtEndpoint is the name of your text box for the API endpoint
            }

            checkedListBox4.Items.Clear();
            try
            {
                string response = client.DownloadString(endpoint);
                var podResponse = JsonConvert.DeserializeObject<PodsResponse>(response);

                var excludedNamespaces = new HashSet<string> { "ingress", "kube-node-lease", "kube-public", "kube-system" };

                foreach (var pods in podResponse.items)
                {
                    if (!excludedNamespaces.Contains(pods.metadata.Namespace))
                    {
                        checkedListBox4.Items.Add("" + pods.metadata.Namespace + "---" + pods.metadata.Name + "");
                    }
                }

               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                client.Dispose();
            }
        }

        private void buttonEliminarPod_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();


            List<string> checkedValues = new List<string>();
            foreach (var item in checkedListBox4.CheckedItems)
            {
                checkedValues.Add(item.ToString());
            }

            if (checkedListBox4.CheckedItems.Count == 0)
            {
                MessageBox.Show("SELECIONE UM POD PARA ELIMINAR !");
            }
            else
            {
                foreach (var item in checkedValues)
                {
                    try
                    {
                        string[] parts = item.Split(new string[] { "---" }, StringSplitOptions.None);

                        string Namespace = parts[0];
                        string Pod = parts[1];

                       

                        
                        string endpoint = "";

                        if (checkBoxToken.Checked)
                        {
                            endpoint = "https://" + textBoxIPPorta.Text + "/api/v1/namespaces/" + Namespace + "/pods/" + Pod +"";

                            string bearerToken = "" + textBoxToken.Text + "";  // Replace this with the actual way to get your token

                            // Add the Authorization header with the bearer token
                            client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

                        }
                        else
                        {
                            endpoint = "http://" + textBoxIPPorta.Text + "/api/v1/namespaces/" + Namespace + "/pods/" + Pod +""; // Assuming txtEndpoint is the name of your text box for the API endpoint
                        }


                        //string endpointDeletePod = "http://" + textBoxIPPorta.Text + "/api/v1/namespaces/" + Namespace + "/pods/" + Pod + "";
                        client.UploadString(endpoint, "DELETE", string.Empty);

                        MessageBox.Show("POD ELIMINADO !");
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("ERRO AO ELIMINAR POD !");
                    }
                    finally
                    {
                        client.Dispose();


                    }




                }
            }
        }

        private void buttonGETNamespacePOD_Click(object sender, EventArgs e)
        {
            ListarNamespaces(comboBoxNamespacePOD);

        }
        private void buttonCriarPod_Click(object sender, EventArgs e)
        {
            string endpointCreateNamespace = "http://" + textBoxIPPorta.Text + "/api/v1/namespaces/"+comboBoxNamespacePOD.Text+"/pods ";
           



            string endpoint = "";
            WebClient client = new WebClient();

            if (checkBoxToken.Checked)
            {
                endpoint = "https://" + textBoxIPPorta.Text + "/api/v1/namespaces/"+comboBoxNamespacePOD.Text+"/pods ";

                string bearerToken = "" + textBoxToken.Text + "";  // Replace this with the actual way to get your token

                // Add the Authorization header with the bearer token
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpoint = "http://" + textBoxIPPorta.Text + "/api/v1/namespaces/"+comboBoxNamespacePOD.Text+"/pods ";// Assuming txtEndpoint is the name of your text box for the API endpoint
            }

            string jsonContent = $@"{{
    ""apiVersion"": ""v1"",
    ""kind"": ""Pod"",
    ""metadata"": {{
        ""name"": ""{textBoxNomePOD.Text}"",
        ""labels"": {{
            ""controller-revision-hash"": ""5b8c6899f6"",
            ""name"": ""{textBoxNomePOD.Text}"",
            ""pod-template-generation"": ""1""
        }}
    }},
    ""spec"": {{
        ""containers"": [
            {{
                ""name"": ""{comboBoxImagemPOD.Text}"",
                ""image"": ""{comboBoxImagemPOD.Text}:latest"",
                ""imagePullPolicy"": ""IfNotPresent"",
                ""ports"": [
                    {{
                        ""containerPort"": {numericUpDownContainerPort.Text}
                    }}
                ]
            }}
        ],
        ""restartPolicy"": ""{comboBoxRestartPolicyPod.Text}""  
    }}
}}";






            string response = "";
            try
            {
                response = client.UploadString(endpoint, "POST", jsonContent);
                Console.WriteLine("Response: " + response);
                MessageBox.Show("POD CRIADO !");
            }
            catch (WebException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                MessageBox.Show("ERRO AO CRIAR POD!");
            }
            finally
            {

                client.Dispose();
                comboBoxNamespacePOD.Text = "";
                textBoxNomePOD.Text = "";
                comboBoxImagemPOD.Text = "";
                comboBoxRestartPolicyPod.Text = "";
                numericUpDownContainerPort.Value = 0;
            }
        }



        #endregion


        #region Deployment
        private void buttonListarDeployment_Click(object sender, EventArgs e)
        {


            WebClient client = new WebClient();
            string endpoint = "";

            if (checkBoxToken.Checked)
            {
                endpoint = "https://" + textBoxIPPorta.Text + "/apis/apps/v1/deployments";

                string bearerToken = "" + textBoxToken.Text + "";  

                
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpoint = "http://" + textBoxIPPorta.Text + "/apis/apps/v1/deployments";
            }


            checkedListBox5.Items.Clear();
            try
            {
                string response = client.DownloadString(endpoint);
                var deploymentResponse = JsonConvert.DeserializeObject<DeploymentsResponse>(response);

                var excludedNamespaces = new HashSet<string> { "ingress", "kube-node-lease", "kube-public", "kube-system" };

                foreach (var deployments in deploymentResponse.items)
                {
                    if (!excludedNamespaces.Contains(deployments.metadata.Namespace))
                    {
                        checkedListBox5.Items.Add("" + deployments.metadata.Namespace + "---" + deployments.metadata.Name + "");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                client.Dispose();
            }
        }

        private void buttonEliminarDeployment_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();


            List<string> checkedValues = new List<string>();
            foreach (var item in checkedListBox5.CheckedItems)
            {
                checkedValues.Add(item.ToString());
            }

            if (checkedListBox5.CheckedItems.Count == 0)
            {
                MessageBox.Show("SELECIONE UM DEPLOYMENT PARA ELIMINAR !");
            }
            else
            {
                foreach (var item in checkedValues)
                {
                    try
                    {
                        string[] parts = item.Split(new string[] { "---" }, StringSplitOptions.None);

                        string Namespace = parts[0];
                        string Deployment = parts[1];

                        string endpoint = "";

                        if (checkBoxToken.Checked)
                        {
                            endpoint = "https://" + textBoxIPPorta.Text + "/apis/apps/v1/namespaces/" + Namespace + "/deployments/" + Deployment + "";

                            string bearerToken = "" + textBoxToken.Text + "";


                            client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

                        }
                        else
                        {
                            endpoint = "http://" + textBoxIPPorta.Text + "/apis/apps/v1/namespaces/" + Namespace + "/deployments/" + Deployment + "";
                        }

                        client.UploadString(endpoint, "DELETE", string.Empty);
                        MessageBox.Show("DEPLOYMENT ELIMINADO !");
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("ERRO AO ELIMINAR DEPLOYMENT !");
                    }
                    finally
                    {
                        client.Dispose();


                    }




                }
            }
        }

        private void buttonGETNamespaceDeployment_Click(object sender, EventArgs e)
        {
            ListarNamespaces(comboBoxNamespaceDeployment);
        }

        private void buttonCriarDeployment_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();

            string jsonContent = $@"{{
  ""apiVersion"": ""apps/v1"",
  ""kind"": ""Deployment"",
  ""metadata"": {{
    ""name"": ""{textBoxNomeDeployment.Text}""
  }},
  ""spec"": {{
    ""replicas"": {numericUpDownReplicasDeployment.Text},
    ""selector"": {{
      ""matchLabels"": {{
        ""app"": ""example""
      }}
    }},
    ""template"": {{
      ""metadata"": {{
        ""labels"": {{
          ""app"": ""example""
        }}
      }},
      ""spec"": {{
        ""containers"": [
          {{
            ""name"": ""{comboBoxImageDeployment.Text}"",
            ""image"": ""{comboBoxImageDeployment.Text}:latest"",
            ""ports"": [
              {{
                ""containerPort"": {numericUpDownContainerPortDeployment.Text}
              }}
            ]
          }}
        ]
      }}
    }}
  }}
}}";

            
            string endpoint = "";

            if (checkBoxToken.Checked)
            {
                endpoint = "https://" + textBoxIPPorta.Text + "/apis/apps/v1/namespaces/"+comboBoxNamespaceDeployment.Text+"/deployments";

                string bearerToken = "" + textBoxToken.Text + "";


                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpoint = "http://" + textBoxIPPorta.Text + "/apis/apps/v1/namespaces/"+comboBoxNamespaceDeployment.Text+"/deployments";
            }




            string response = "";
            try
            {
                response = client.UploadString(endpoint, "POST", jsonContent);
                Console.WriteLine("Response: " + response);
                MessageBox.Show("DEPLOYMENT CRIADO !");
            }
            catch (WebException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                MessageBox.Show("ERRO AO CRIAR DEPLOYMENT!");
            }
            finally
            {

                client.Dispose();
                comboBoxNamespaceDeployment.Text = "";
                textBoxNomeDeployment.Text = "";
                comboBoxImageDeployment.Text = "";
                numericUpDownContainerPortDeployment.Value = 0;
                numericUpDownReplicasDeployment.Value = 0;

            }
        }


        #endregion


        #region Service
        private void buttonListarService_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();

           
            string endpoint = "";

            if (checkBoxToken.Checked)
            {
                endpoint = "https://" + textBoxIPPorta.Text + "/api/v1/services";

                string bearerToken = "" + textBoxToken.Text + "";


                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpoint = "http://" + textBoxIPPorta.Text + "/api/v1/services";
            }

            checkedListBox6.Items.Clear();
            try
            {
                string response = client.DownloadString(endpoint);
                var servicesResponse = JsonConvert.DeserializeObject<ServicesResponse>(response);

                var excludedNamespaces = new HashSet<string> { "ingress", "kube-node-lease", "kube-public", "kube-system" };

                foreach (var services in servicesResponse.items)
                {
                    if (!excludedNamespaces.Contains(services.metadata.Namespace))
                    {
                        checkedListBox6.Items.Add("" + services.metadata.Namespace + "---" + services.metadata.Name + "");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                client.Dispose();
            }
        }


        private void buttonEliminarService_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();


            List<string> checkedValues = new List<string>();
            foreach (var item in checkedListBox6.CheckedItems)
            {
                checkedValues.Add(item.ToString());
            }

            if (checkedListBox6.CheckedItems.Count == 0)
            {
                MessageBox.Show("SELECIONE UM SERVICE PARA ELIMINAR !");
            }
            else
            {
                foreach (var item in checkedValues)
                {
                    try
                    {
                        string[] parts = item.Split(new string[] { "---" }, StringSplitOptions.None);

                        string Namespace = parts[0];
                        string Service = parts[1];

                        string endpoint = "";

                        if (checkBoxToken.Checked)
                        {
                            endpoint = "https://" + textBoxIPPorta.Text + "/api/v1/namespaces/" + Namespace + "/services/" + Service + "";

                            string bearerToken = "" + textBoxToken.Text + "";


                            client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

                        }
                        else
                        {
                            endpoint = "http://" + textBoxIPPorta.Text + "/api/v1/namespaces/" + Namespace + "/services/" + Service + "";
                        }


                        client.UploadString(endpoint, "DELETE", string.Empty);
                        MessageBox.Show("SERVICE ELIMINADO !");
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("ERRO AO ELIMINAR SERVICE !");
                    }
                    finally
                    {
                        client.Dispose();


                    }




                }
            }
        }

        private void buttonGETNamespaceService_Click(object sender, EventArgs e)
        {
            ListarNamespaces(comboBoxNamespaceService);
        }


        private void buttonCriarService_Click(object sender, EventArgs e) 
        {
            


            string jsonContent = $@"{{
  ""apiVersion"": ""v1"",
  ""kind"": ""Service"",
  ""metadata"": {{
    ""name"": ""{textBoxNomeService.Text}""
  }},
  ""spec"": {{
    ""selector"": {{
      ""app"": ""example""
    }},
    ""ports"": [
      {{
        ""protocol"": ""{comboBoxProtocoloService.Text}"",
        ""port"": {numericUpDownPortService.Value},
        ""targetPort"": {numericUpDownTargetPortService.Value}
      }}
    ],
    ""type"": ""{comboBoxServiceTypeService.Text}""
    
  }}
}}";

            WebClient client = new WebClient();


            string endpoint = "";

            if (checkBoxToken.Checked)
            {
                endpoint = "https://" + textBoxIPPorta.Text + "/api/v1/namespaces/" + comboBoxNamespaceService.Text + "/services";

                string bearerToken = "" + textBoxToken.Text + "";


                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + bearerToken;

            }
            else
            {
                endpoint = "http://" + textBoxIPPorta.Text + "/api/v1/namespaces/" + comboBoxNamespaceService.Text + "/services ";
            }



            string response = "";
            try
            {
                response = client.UploadString(endpoint, "POST", jsonContent);
                Console.WriteLine("Response: " + response);
                MessageBox.Show("SERVICE CRIADO !");
            }
            catch (WebException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                MessageBox.Show("ERRO AO CRIAR SERVICE!");
            }
            finally
            {

                client.Dispose();
                comboBoxNamespaceService.Text = "";
                textBoxNomeService.Text = "";
                comboBoxProtocoloService.Text = "";
                numericUpDownPortService.Value = 0;
                numericUpDownTargetPortService.Value = 0;
                comboBoxServiceTypeService.Text = "";
            }
        }

        #endregion

        
        

        
        
    }


}
