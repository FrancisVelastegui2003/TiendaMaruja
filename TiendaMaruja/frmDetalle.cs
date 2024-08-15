using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace TiendaMaruja
{
    public partial class frmDetalle : Form
    {
        private decimal totalFactura = 0m;  // Acumulador para el total de la factura
        private List<FacturaDetalle> facturaDetalles = new List<FacturaDetalle>();  // Lista para almacenar los detalles de la factura
        private int idFactura;  // Id de la factura generada
        private string sucursalId; // ID de la sucursal (SUC001 o SUC002)

        public frmDetalle(int idFactura, string sucursalId)
        {
            InitializeComponent();
            this.idFactura = idFactura;  // Asignar el Id de la factura creada
            this.sucursalId = sucursalId; // Asignar el ID de la sucursal

            CargarProductos();  // Cargar productos al inicializar el formulario
        }

        private void CargarProductos()
        {
            // Limpiar el ComboBox antes de cargar los productos
            cmbProducto.Items.Clear();

            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open();

                    // Consulta para obtener todos los productos de la tabla Producto
                    string query = "SELECT ID_Producto, Nombre_Producto, Precio_Producto FROM Producto";

                    SqlCommand cmd = new SqlCommand(query, connection);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        // Manejar posibles valores nulos en la base de datos
                        string nombreProducto = reader["Nombre_Producto"] != DBNull.Value ? reader["Nombre_Producto"].ToString() : "Producto sin nombre";
                        decimal precioProducto = reader["Precio_Producto"] != DBNull.Value ? Convert.ToDecimal(reader["Precio_Producto"], CultureInfo.InvariantCulture) : 0m;

                        // Cargar los productos en el ComboBox
                        cmbProducto.Items.Add(new ComboBoxItem
                        {
                            Text = nombreProducto,
                            Value = reader["ID_Producto"].ToString(),
                            Price = precioProducto
                        });
                    }

                    if (cmbProducto.Items.Count > 0)
                    {
                        cmbProducto.SelectedIndex = 0; // Seleccionar el primer producto por defecto
                    }
                    else
                    {
                        MessageBox.Show("No hay productos disponibles.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message);
            }
        }

        private void btnTotal_Click(object sender, EventArgs e)
        {
            // Validar los campos antes de calcular
            if (cmbProducto.SelectedItem == null || string.IsNullOrWhiteSpace(txtUnidades.Text) || string.IsNullOrWhiteSpace(txtLinea.Text))
            {
                MessageBox.Show("Debe seleccionar un producto, ingresar las unidades y el detalle.");
                return;
            }

            if (!int.TryParse(txtUnidades.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int unidades))
            {
                MessageBox.Show("Las unidades deben ser un número válido.");
                return;
            }

            if (unidades <= 0)
            {
                MessageBox.Show("Las unidades deben ser mayores a 0.");
                return;
            }

            var selectedItem = (ComboBoxItem)cmbProducto.SelectedItem;
            decimal precioProducto = selectedItem.Price;

            // Calcular el total parcial
            decimal totalParcial = precioProducto * unidades;

            // Sumar al total de la factura
            totalFactura += totalParcial;

            // Agregar el detalle de la factura a la lista
            facturaDetalles.Add(new FacturaDetalle
            {
                IdFactura = idFactura.ToString(),
                IdProducto = selectedItem.Value,
                NombreProducto = selectedItem.Text,
                Unidades = unidades,
                PrecioUnidad = precioProducto,
                LineaDetalle = txtLinea.Text,
                PrecioTotal = totalParcial
            });

            // Registrar el producto en la base de datos
            RegistrarProductoEnFactura(idFactura, selectedItem.Value, unidades, txtLinea.Text, totalParcial);

            // Limpiar los campos para permitir agregar otro producto
            MessageBox.Show($"Producto agregado: {selectedItem.Text}\nTotal parcial: {totalParcial.ToString("C", CultureInfo.InvariantCulture)}\nTotal acumulado: {totalFactura.ToString("C", CultureInfo.InvariantCulture)}");
            LimpiarCampos();
        }

        private void RegistrarProductoEnFactura(int idFactura, string idProducto, int unidades, string lineaDetalle, decimal precioTotal)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open();

                    // Seleccionar la tabla de detalle de factura según la sucursal
                    string tablaDetalleFactura = sucursalId == "QUITO" ? "Detalle_Factura_Quito" : "Detalle_Factura_Guayaquil";

                    string query = $"INSERT INTO {tablaDetalleFactura} (ID_Factura, ID_Producto, Unidades, Precio, Linea) " +
                                   "VALUES (@IdFac, @IdProd, @Unidades, @Precio, @Linea)";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@IdFac", idFactura);
                    cmd.Parameters.AddWithValue("@IdProd", idProducto);
                    cmd.Parameters.AddWithValue("@Unidades", unidades);
                    cmd.Parameters.AddWithValue("@Precio", precioTotal);
                    cmd.Parameters.AddWithValue("@Linea", lineaDetalle);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar el producto en la factura: " + ex.Message);
            }
        }

        private void LimpiarCampos()
        {
            txtLinea.Clear();
            txtUnidades.Clear();
            cmbProducto.SelectedIndex = -1;  // Deseleccionar el producto
        }

        private void btnResumen_Click(object sender, EventArgs e)
        {
            // Actualizar el total de la factura en la tabla Factura
            ActualizarMontoTotalFactura();

            // Ya se ha registrado todo, ahora pasar al siguiente formulario
            frmTotal frmTotal = new frmTotal(idFactura, sucursalId);
            frmTotal.Show();
            this.Hide();
        }

        private void ActualizarMontoTotalFactura()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open();

                    // Seleccionar la tabla de factura según la sucursal
                    string tablaFactura = sucursalId == "QUITO" ? "Factura_Quito" : "Factura_Guayaquil";

                    // Actualizar el monto total de la factura
                    string query = $"UPDATE {tablaFactura} SET Monto = @Monto WHERE ID_Factura = @IdFac";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Monto", totalFactura);
                    cmd.Parameters.AddWithValue("@IdFac", idFactura);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el monto total de la factura: " + ex.Message);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            frmVerificar frmVerificar = new frmVerificar();
            frmVerificar.Show();
            this.Hide();
        }
    }

    // Clase para almacenar el texto y valor en el ComboBox
    public class ComboBoxItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public decimal Price { get; set; }

        public override string ToString()
        {
            return Text;  // Mostrar el nombre del producto en el ComboBox
        }
    }

    // Clase para almacenar los detalles de la factura
    public class FacturaDetalle
    {
        public string IdFactura { get; set; }
        public string IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Unidades { get; set; }
        public decimal PrecioUnidad { get; set; }
        public string LineaDetalle { get; set; }
        public decimal PrecioTotal { get; set; }
    }
}
