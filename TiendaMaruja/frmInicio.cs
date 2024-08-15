using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TiendaMaruja
{
    public partial class frmInicio : Form
    {
        public frmInicio()
        {
            InitializeComponent();
            // Configurar el TextBox de la contraseña para que muestre asteriscos
            txtContrasena.PasswordChar = '*';
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            // Capturar los valores de los TextBox
            string userInput = txtUsuario.Text;
            string passInput = txtContrasena.Text;

            // Determinar el servidor y la base de datos basada en el nombre del equipo
            string serverName;
            string databaseName;

            if (Environment.MachineName.Equals("DESKTOP-GN7PLSE", StringComparison.OrdinalIgnoreCase))
            {
                serverName = "DESKTOP-GN7PLSE\\MSSQLSERVER2022";
                databaseName = "TiendaMaruja_Quito";  // Base de datos para este nodo
            }
            else if (Environment.MachineName.Equals("DESKTOP-MMPNRD5", StringComparison.OrdinalIgnoreCase))
            {
                serverName = "DESKTOP-MMPNRD5\\MSSQLSERVER2022";
                databaseName = "TiendaMaruja_Guayaquil";  // Base de datos para el otro nodo
            }
            else
            {
                MessageBox.Show("El nombre del servidor no coincide con ninguno de los nodos configurados.");
                return;
            }

            // Crear la cadena de conexión con las credenciales ingresadas por el usuario y el nombre de la base de datos
            GlobalConfig.ConnectionString = $"Server={serverName};Database={databaseName};User Id={userInput};Password={passInput};";

            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open(); // Intenta abrir la conexión

                    // Si la conexión es exitosa, muestra un mensaje de éxito y abre el siguiente formulario
                    MessageBox.Show("Ingreso exitoso");
                    frmConsultas menu = new frmConsultas(); // Abre el formulario de consultas
                    menu.Show();
                    this.Hide(); // Ocultar el formulario actual
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Usuario o contraseña incorrectos o error al conectar con la base de datos: " + "\n" + ex.Message);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Cerrar la aplicación
        }
    }
}
