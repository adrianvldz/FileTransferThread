using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
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
using FileTransfer.Clases;
using System.Threading;

namespace FileTransfer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int puerto = 8000;
        private readonly string carpetaEnvio = @"C:\FileTransfer\Send";
        private readonly string carpetaRecepcion = @"C:\FileTransfer\Receive";

        // Diccionario que asocia IPs con nombres de PC
        private readonly Dictionary<string, string> nombrePorIP = new Dictionary<string, string>
    {
        { "172.16.128.10", "PC-Jony" },
        { "172.16.128.29", "PC-Adrian" },
        { "192.168.1.4", "PC-3" }
    };

        private Servidor servidor;
        private Cliente cliente;

        public MainWindow()
        {
            InitializeComponent();
            Directory.CreateDirectory(carpetaEnvio);
            Directory.CreateDirectory(carpetaRecepcion);

            servidor = new Servidor(puerto, carpetaRecepcion, Log, ActualizarProgreso);
            cliente = new Cliente(puerto, Log, ActualizarProgreso);
        }

        private void StartTransfer_Click(object sender, RoutedEventArgs e)
        {
            servidor.Iniciar();

            Thread hiloTransferencia = new Thread(() =>
            {
                int index = 0;
                foreach (var entry in nombrePorIP)  // Iteramos sobre el diccionario
                {
                    string ip = entry.Key;
                    string nombrePC = entry.Value;  // Nombre asociado a la IP

                    foreach (var rutaArchivo in Directory.GetFiles(carpetaEnvio))
                    {
                        Archivo archivo = new Archivo(rutaArchivo);
                        AgregarProgressBar(index, archivo);
                        cliente.EnviarArchivo(ip, archivo, index, nombrePC);  // Pasamos la IP y el nombre
                        index++;
                    }
                }
            });

            hiloTransferencia.Start();
        }

        private void AgregarProgressBar(int index, Archivo archivo)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar progressBar = new ProgressBar
                {
                    Name = $"progress_{index}",
                    Width = 200,
                    Height = 20,
                    Minimum = 0,
                    Maximum = 1,
                    Value = 0
                };

                ProgressPanel.Children.Add(progressBar);
            });
        }

        private void ActualizarProgreso(int index, double progreso)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar progressBar = ProgressPanel.Children.OfType<ProgressBar>().ElementAt(index);
                progressBar.Value = progreso;
            });
        }

        private void Log(string mensaje)
        {
            Dispatcher.Invoke(() => LogBox.Items.Add($"{DateTime.Now}: {mensaje}"));
        }
    }



}
