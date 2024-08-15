using System;
using System.Data.SqlClient;
using System.Transactions;
using System.Windows.Forms;

namespace TiendaMaruja
{
    public partial class frmEmpleados : Form
    {
        public frmEmpleados()
        {
            InitializeComponent();
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            // Capturar los valores ingresados por el usuario
            string cedula = txtCedula.Text; // Capturar el ID del empleado desde txtCedula
            string nombre = txtNombre.Text;
            string apellido = txtApellido.Text;
            string direccion = txtDirecc.Text;
            string telefono = txtTelef.Text;
            string sucursal = cmbSucursal.SelectedItem.ToString();

            // Validación básica de los datos ingresados
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(direccion) ||
                string.IsNullOrWhiteSpace(telefono) || string.IsNullOrWhiteSpace(sucursal))
            {
                MessageBox.Show("Todos los campos son obligatorios.");
                return;
            }

            // Determinar la tabla en función de la sucursal seleccionada
            string tablaEmpleado = sucursal == "QUITO" ? "Empleado_Quito" : (sucursal == "GUAYAQUIL" ? "Empleado_Guayaquil" : null);
            string tablaTelefono = sucursal == "QUITO" ? "Telefonos_Empleado_Quito" : (sucursal == "GUAYAQUIL" ? "Telefonos_Empleado_Guayaquil" : null);

            if (tablaEmpleado == null || tablaTelefono == null)
            {
                MessageBox.Show("Sucursal seleccionada no válida.");
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

                        // Insertar el nuevo empleado en la tabla Empleado correspondiente con el ID del empleado proporcionado
                        string queryEmpleado = $"INSERT INTO {tablaEmpleado} (ID_Empleado, ID_Sucursal, Nombre_Empleado, Apellido_Empleado, Direccion_Empleado) " +
                                               "VALUES (@IdEmp, @IdSuc, @nombre, @apellido, @direccion);";
                        SqlCommand cmdEmpleado = new SqlCommand(queryEmpleado, connection);
                        cmdEmpleado.Parameters.AddWithValue("@IdEmp", cedula);
                        cmdEmpleado.Parameters.AddWithValue("@IdSuc", sucursal == "QUITO" ? "SUC001" : "SUC002");
                        cmdEmpleado.Parameters.AddWithValue("@nombre", nombre);
                        cmdEmpleado.Parameters.AddWithValue("@apellido", apellido);
                        cmdEmpleado.Parameters.AddWithValue("@direccion", direccion);

                        // Ejecutar la inserción del empleado
                        cmdEmpleado.ExecuteNonQuery();

                        // Insertar el teléfono del empleado en la tabla correspondiente
                        string queryTelefono = $"INSERT INTO {tablaTelefono} (ID_Empleado, Numero) VALUES (@IdEmp, @telefono)";
                        SqlCommand cmdTelefono = new SqlCommand(queryTelefono, connection);
                        cmdTelefono.Parameters.AddWithValue("@IdEmp", cedula);
                        cmdTelefono.Parameters.AddWithValue("@telefono", telefono);
                        cmdTelefono.ExecuteNonQuery();
                    }

                    // Completar la transacción si todo fue exitoso
                    scope.Complete();
                    MessageBox.Show("Empleado registrado con éxito.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al registrar el empleado: " + ex.Message);
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            frmConsultas menu = new frmConsultas();
            menu.Show();
            this.Hide();
        }

        private void cmbSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Sucursal seleccionada: " + cmbSucursal.Text);
        }
    }
}
