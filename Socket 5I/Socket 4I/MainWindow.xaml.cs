using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Socket_4I
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Di Girolamo Martina 5I 04/04/2023


        Socket socket;
        DispatcherTimer dTimer;
        IPEndPoint remote_endpoint;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            /*
            * Inizializzo una nuova socket.
            * Il parametro AddressFamily.Internetwork dichiara che l'indirizzo IP dev'essere di tipo IPV4
            * Il parametro SocketType.Dgram supporta datagrammi non affidabili di lunghezza fissa senza connessione
            * Il parametro ProtocolType.Udp dichiara il tipo di protocollo per la comunicazione
            */
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //Dichiaro che qualisiasi indirizzo IP è valido per la connessione
            IPAddress local_address = IPAddress.Any;

            //Dichiaro l'indirizzo ip e la porta al quale voglio ricevere messaggi
            IPEndPoint local_endpoint = new IPEndPoint(local_address, 10000);

            //Associo la socket all'endpoint
            socket.Bind(local_endpoint);

            //La socket non è in modalità di blocco
            socket.Blocking = false;

            //La modalità di broadcast è attiva
            socket.EnableBroadcast = true;

            //creazione thread per la ricezione dei messaggi
            Thread ricezione = new Thread(Ricevi);
            ricezione.Start();
        }

        //Thread sempre in ascolto
        private void Ricevi()
        {
            int nBytes = 0;
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            do
            {
                if ((nBytes = socket.Available) > 0)
                {
                    //ricezione dei caratteri in attesa
                    byte[] buffer = new byte[nBytes];

                    //metodo per ricevere messaggi
                    nBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint);

                    //contiene l'indirizzo ip del mittente
                    string from = ((IPEndPoint)remoteEndPoint).Address.ToString();

                    //converto da byte a UTF8
                    string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes);

                    //utilizzato per invocare un metodo appartenente al mainwindow
                    Dispatcher.Invoke(() =>
                    {
                        lstMessaggi.Items.Add(from + ": " + messaggio);
                    });
                   
                }

                //per non sovraccaricare la cpu metto il thread in sleep per 150 ms
                Thread.Sleep(150);
            } while (true);
        }
        

        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            //converto l'ip scritto nella textbox in un oggetto IPAddress
            IPAddress remote_address = IPAddress.Parse(txtTo.Text);

            //inizializzo l'endpoint a cui voglio inviare i messaggi
            remote_endpoint = new IPEndPoint(remote_address, 11000);

            //trasformo da stringa UTF8 ad array di byte
            byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text);

            lstMessaggi.Items.Add("Io: " + txtMessaggio.Text);

            //invio il messaggio all'endpoint precedentemente creato
            socket.SendTo(messaggio, remote_endpoint);

        }
    }
}
