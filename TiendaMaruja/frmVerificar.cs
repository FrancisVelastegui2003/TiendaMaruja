using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TiendaMaruja
{
    public partial class frmVerificar : Form
    {
        public frmVerificar()
        {
            InitializeComponent();
            CargarSucursales();
        }

        // Cargar las sucursales en el ComboBox
        private void CargarSucursales()
        {
            cmbSucursal.Items.Clear();
            cmbSucursal.Items.Add("QUITO");
            cmbSucursal.Items.Add("GUAYAQUIL");
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string cedula = txtCedula.Text;
            string sucursal = cmbSucursal.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(sucursal))
            {
                MessageBox.Show("Por favor, ingrese una cédula válida y seleccione una sucursal.");
                return;
            }

            // La consulta es la misma para ambas sucursales, porque los clientes son compartidos
            string query = "SELECT ID_Cliente, Nombre_Cliente, Apellido_Cliente, Direccion_Cliente, Telefono_Cliente " +
                           "FROM Cliente WHERE ID_Cliente = @cedula";

            // Conectar a la base de datos para buscar la cédula y mostrar la información en el DataGridView
            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@cedula", cedula);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        // Mostrar los datos del cliente en el DataGridView
                        dataGridView1.DataSource = dt;
                        MessageBox.Show("Cliente encontrado. Puede proceder con la venta.");
                        EnableVentaFields(true);
                    }
                    else
                    {
                        // Si no se encuentra la cédula, redirige al formulario de registro de cliente
                        MessageBox.Show("Cédula no encontrada. Debe registrar al cliente.");
                        frmCliente frmCliente = new frmCliente();
                        frmCliente.Show();
                        this.Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar el cliente: " + ex.Message);
            }
        }

        private void EnableVentaFields(bool enable)
        {
            // Habilitar o deshabilitar los campos para realizar la venta
            txtCEmp.Enabled = enable;
            dtFecha.Enabled = enable;
            cmbSucursal.Enabled = enable;
            btnVenta.Enabled = enable;
            dataGridView1.Enabled = enable;
        }

        private void btnVenta_Click(object sender, EventArgs e)
        {
            string idCliente = txtCedula.Text;

            // Verificar si el IdCliente existe en la tabla Cliente
            if (!ClienteExiste(idCliente))
            {
                MessageBox.Show("El cliente no existe en la base de datos.");
                return;
            }

            string idEmpleado = txtCEmp.Text;
            string fecha = dtFecha.Value.ToString("yyyy-MM-dd");
            string sucursal = cmbSucursal.SelectedItem?.ToString();
            string tablaFactura = sucursal == "QUITO" ? "Factura_Quito" : "Factura_Guayaquil";

            // Conectar a la base de datos y realizar la inserción en la tabla Factura
            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open();

                    // Obtener el último ID_Factura y sumar 1
                    SqlCommand cmdLastId = new SqlCommand($"SELECT ISNULL(MAX(ID_Factura), 0) + 1 FROM {tablaFactura}", connection);
                    int newIdFactura = (int)cmdLastId.ExecuteScalar();

                    // Iniciar la transacción
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insertar en la tabla Factura de la sucursal correspondiente
                            string queryFactura = $"INSERT INTO {tablaFactura} (ID_Factura, ID_Cliente, ID_Empleado, ID_Sucursal, Fecha_Emision, Monto) " +
                                                  "VALUES (@IdFactura, @IdCli, @IdEmp, @IdSuc, @fecha, @monto);"; // Aquí se establece el Monto inicial en 0.00
                            SqlCommand cmdFactura = new SqlCommand(queryFactura, connection, transaction);
                            cmdFactura.Parameters.AddWithValue("@IdFactura", newIdFactura);
                            cmdFactura.Parameters.AddWithValue("@IdCli", idCliente);
                            cmdFactura.Parameters.AddWithValue("@IdEmp", idEmpleado);
                            cmdFactura.Parameters.AddWithValue("@IdSuc", sucursal == "QUITO" ? "SUC001" : "SUC002");
                            cmdFactura.Parameters.AddWithValue("@fecha", fecha);
                            cmdFactura.Parameters.AddWithValue("@monto", 0.00M); // Se pasa el valor 0.00 como Monto

                            cmdFactura.ExecuteNonQuery();

                            // Commit de la transacción
                            transaction.Commit();

                            // Mostrar un mensaje de éxito
                            MessageBox.Show("Venta registrada exitosamente. ID de Factura: " + newIdFactura);

                            // Pasar a la pantalla de detalles de la venta
                            frmDetalle frmDetalle = new frmDetalle(newIdFactura, sucursal);
                            frmDetalle.Show();
                            this.Hide();
                        }
                        catch (Exception ex)
                        {
                            // Si hay un error, hacer rollback de la transacción
                            transaction.Rollback();
                            MessageBox.Show("Error al registrar la venta: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
            }
        }

        private bool ClienteExiste(string idCliente)
        {
            // La consulta es la misma para ambas sucursales, porque los clientes son compartidos
            string query = "SELECT COUNT(1) FROM Cliente WHERE ID_Cliente = @IdCliente";

            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al verificar la existencia del cliente: " + ex.Message);
                return false;
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            frmConsultas menu = new frmConsultas();
            menu.Show();
            this.Hide();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verificar que la celda seleccionada es válida
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Obtener el valor de ID_Cliente de la fila seleccionada
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                string idCliente = row.Cells["ID_Cliente"].Value.ToString();

                // Mostrar el ID_Cliente en el TextBox correspondiente
                txtCedula.Text = idCliente;
            }
        }
    }
}
