using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace TiendaMaruja
{
    public partial class frmProducto : Form
    {
        public frmProducto()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            // Capturar los valores ingresados por el usuario
            string idProducto = txtCodigo.Text;
            string nombreProducto = txtNombre.Text;
            string precioText = txtPrecio.Text;

            // Validación básica de los datos ingresados
            if (string.IsNullOrWhiteSpace(idProducto) || string.IsNullOrWhiteSpace(nombreProducto) ||
                string.IsNullOrWhiteSpace(precioText) ||
                !decimal.TryParse(precioText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precio))
            {
                MessageBox.Show("Todos los campos son obligatorios y el precio debe ser un valor decimal válido.");
                return;
            }

            try
            {
                // Conectar a la base de datos
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open();

                    // Insertar el nuevo producto en la tabla Producto
                    string queryProducto = "INSERT INTO Producto (ID_Producto, Nombre_Producto, Precio_Producto) " +
                                           "VALUES (@idProducto, @nombreProducto, @precio);";
                    SqlCommand cmdProducto = new SqlCommand(queryProducto, connection);
                    cmdProducto.Parameters.AddWithValue("@idProducto", idProducto);
                    cmdProducto.Parameters.AddWithValue("@nombreProducto", nombreProducto);
                    cmdProducto.Parameters.AddWithValue("@precio", precio);

                    // Ejecutar el comando
                    cmdProducto.ExecuteNonQuery();

                    MessageBox.Show("Producto ingresado correctamente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al ingresar el producto: " + ex.Message);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            frmConsultas menu = new frmConsultas();
            menu.Show();
            this.Hide();
        }
    }
}
