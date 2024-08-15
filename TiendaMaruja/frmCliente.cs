using System;
using System.Data.SqlClient;
using System.Transactions;
using System.Windows.Forms;

namespace TiendaMaruja
{
    public partial class frmCliente : Form
    {
        public frmCliente()
        {
            InitializeComponent();
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            // Capturar los valores ingresados por el usuario
            string cedula = txtCedula.Text;
            string nombre = txtNombre.Text;
            string apellido = txtApellido.Text;
            string direccion = txtDirecc.Text;
            string telefono = txtTelef.Text;

            // Validación básica de los datos ingresados
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(direccion) ||
                string.IsNullOrWhiteSpace(telefono))
            {
                MessageBox.Show("Todos los campos son obligatorios.");
                return;
            }

            // Crear el alcance de la transacción distribuida
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    // Conectar a la base de datos
                    using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                    {
                        connection.Open();

                        // Insertar el nuevo cliente en la tabla Cliente
                        string queryCliente = "INSERT INTO Cliente (ID_Cliente, Nombre_Cliente, Apellido_Cliente, Direccion_Cliente, Telefono_Cliente) " +
                                              "VALUES (@cedula, @nombre, @apellido, @direccion, @telefono);";
                        SqlCommand cmdCliente = new SqlCommand(queryCliente, connection);
                        cmdCliente.Parameters.AddWithValue("@cedula", cedula);
                        cmdCliente.Parameters.AddWithValue("@nombre", nombre);
                        cmdCliente.Parameters.AddWithValue("@apellido", apellido);
                        cmdCliente.Parameters.AddWithValue("@direccion", direccion);
                        cmdCliente.Parameters.AddWithValue("@telefono", telefono);

                        cmdCliente.ExecuteNonQuery();
                    }

                    // Completar la transacción si todo fue exitoso
                    scope.Complete();
                    MessageBox.Show("Cliente registrado correctamente.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al registrar el cliente: " + ex.Message);
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            frmConsultas frmConsultas = new frmConsultas();
            frmConsultas.Show();
            this.Hide();
        }
    }
}
