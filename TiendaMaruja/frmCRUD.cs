using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using System.Windows.Forms;

namespace TiendaMaruja
{
    public partial class frmCRUD : Form
    {
        public frmCRUD()
        {
            InitializeComponent();
            CargarSucursales();
            cmbSucursal.SelectedIndexChanged += new EventHandler(CmbSucursal_SelectedIndexChanged);
        }

        // Cargar las sucursales en el ComboBox
        private void CargarSucursales()
        {
            cmbSucursal.Items.Clear();
            cmbSucursal.Items.Add("QUITO");
            cmbSucursal.Items.Add("GUAYAQUIL");
        }

        // Cargar las tablas en el ComboBox dependiendo de la sucursal seleccionada
        private void CargarTablas()
        {
            cmbTabla.Items.Clear();

            string sucursal = cmbSucursal.SelectedItem?.ToString();

            if (sucursal == "QUITO")
            {
                cmbTabla.Items.Add("Cliente");
                cmbTabla.Items.Add("Empleado_Quito");
                cmbTabla.Items.Add("Telefonos_Empleado_Quito");
                cmbTabla.Items.Add("Factura_Quito");
                cmbTabla.Items.Add("Detalle_Factura_Quito");
                cmbTabla.Items.Add("Producto");
                cmbTabla.Items.Add("Inventario_Quito");
            }
            else if (sucursal == "GUAYAQUIL")
            {
                cmbTabla.Items.Add("Cliente");
                cmbTabla.Items.Add("Empleado_Guayaquil");
                cmbTabla.Items.Add("Telefonos_Empleado_Guayaquil");
                cmbTabla.Items.Add("Factura_Guayaquil");
                cmbTabla.Items.Add("Detalle_Factura_Guayaquil");
                cmbTabla.Items.Add("Producto");
                cmbTabla.Items.Add("Inventario_Guayaquil");
            }
        }

        // Evento que ocurre cuando se selecciona una sucursal
        private void CmbSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarTablas();
            CargarDatos();  // Cargar los datos de la tabla seleccionada en la sucursal
        }

        // Mostrar datos en el DataGridView basado en la selección de sucursal y tabla
        private void CargarDatos()
        {
            string sucursal = cmbSucursal.SelectedItem?.ToString();
            string tabla = cmbTabla.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(sucursal) || string.IsNullOrWhiteSpace(tabla))
            {
                MessageBox.Show("Seleccione una sucursal y una tabla.");
                return;
            }

            string query;

            // Determinar si usar la vista particionada o la tabla local
            switch (tabla)
            {
                case "Cliente":
                case "Producto":
                    query = "SELECT * FROM " + tabla;  // Acceso global a clientes y productos
                    break;
                case "Inventario_Quito":
                case "Inventario_Guayaquil":
                    query = "SELECT * FROM v_inventario_global";  // Vista particionada para inventario
                    break;
                case "Factura_Quito":
                case "Factura_Guayaquil":
                    query = "SELECT * FROM v_factura_global";  // Vista particionada para facturas
                    break;
                case "Detalle_Factura_Quito":
                case "Detalle_Factura_Guayaquil":
                    query = "SELECT * FROM v_detalle_factura_global";  // Vista particionada para detalle de facturas
                    break;
                default:
                    query = "SELECT * FROM " + tabla;  // Para Empleado y Telefonos
                    break;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos: " + ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarDatos();
        }

        private void cmbSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarDatos();
        }

        // Lógica para actualizar registros
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            string sucursal = cmbSucursal.SelectedItem?.ToString();
            string tabla = cmbTabla.SelectedItem?.ToString();
            string id = InputBox.Show("Ingrese el ID del registro que desea actualizar:", "Actualizar Registro");

            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Debe ingresar un ID válido.");
                return;
            }

            string primaryKeyColumn = ObtenerClavePrimaria(tabla);

            if (string.IsNullOrEmpty(primaryKeyColumn))
            {
                MessageBox.Show("No se pudo determinar la clave primaria para la tabla seleccionada.");
                return;
            }

            // Ejemplo de columnas que se podrían actualizar
            string columnaAActualizar = InputBox.Show("Ingrese el nombre de la columna que desea actualizar:", "Actualizar Columna");
            string nuevoValor = InputBox.Show("Ingrese el nuevo valor:", "Actualizar Valor");

            if (string.IsNullOrEmpty(columnaAActualizar) || string.IsNullOrEmpty(nuevoValor))
            {
                MessageBox.Show("Debe ingresar una columna y un valor válidos.");
                return;
            }

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                    {
                        connection.Open();

                        // Actualizar solo en la tabla seleccionada
                        string query = $"UPDATE {tabla} SET {columnaAActualizar} = @nuevoValor WHERE {primaryKeyColumn} = @id";

                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@nuevoValor", nuevoValor);

                        int affectedRows = cmd.ExecuteNonQuery();
                        if (affectedRows == 0)
                        {
                            MessageBox.Show("No se encontró un registro con ese ID.");
                        }
                        else
                        {
                            scope.Complete();
                            MessageBox.Show("Registro actualizado correctamente.");
                        }
                    }
                    CargarDatos();  // Recargar los datos para reflejar los cambios
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Violación de restricción de clave foránea
                {
                    MessageBox.Show("No se puede actualizar este registro porque está referenciado en otra tabla.");
                }
                else
                {
                    MessageBox.Show("Error al actualizar el registro: " + ex.Message);
                }
            }
        }

        // Lógica para eliminar registros
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            string sucursal = cmbSucursal.SelectedItem?.ToString();
            string tabla = cmbTabla.SelectedItem?.ToString();
            string id = InputBox.Show("Ingrese el ID del registro que desea eliminar:", "Eliminar Registro");

            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Debe ingresar un ID válido.");
                return;
            }

            string primaryKeyColumn = ObtenerClavePrimaria(tabla);

            if (string.IsNullOrEmpty(primaryKeyColumn))
            {
                MessageBox.Show("No se pudo determinar la clave primaria para la tabla seleccionada.");
                return;
            }

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                    {
                        connection.Open();

                        // Eliminar solo en la tabla seleccionada
                        string query = $"DELETE FROM {tabla} WHERE {primaryKeyColumn} = @id";

                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@id", id);

                        int affectedRows = cmd.ExecuteNonQuery();
                        if (affectedRows == 0)
                        {
                            MessageBox.Show("No se encontró un registro con ese ID.");
                        }
                        else
                        {
                            scope.Complete();
                            MessageBox.Show("Registro eliminado correctamente.");
                        }
                    }
                    CargarDatos();  // Recargar los datos para reflejar los cambios
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Violación de restricción de clave foránea
                {
                    MessageBox.Show("No se puede eliminar este registro porque está referenciado en otra tabla.");
                }
                else
                {
                    MessageBox.Show("Error al eliminar el registro: " + ex.Message);
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            frmConsultas menu = new frmConsultas();
            menu.Show();
            this.Hide();
        }

        // Método para obtener la clave primaria basada en la tabla seleccionada
        private string ObtenerClavePrimaria(string tabla)
        {
            switch (tabla)
            {
                case "Cliente":
                    return "ID_Cliente";
                case "Empleado_Quito":
                case "Empleado_Guayaquil":
                    return "ID_Empleado";
                case "Telefonos_Empleado_Quito":
                case "Telefonos_Empleado_Guayaquil":
                    return "ID_Empleado";  // Parte de la clave primaria compuesta
                case "Factura_Quito":
                case "Factura_Guayaquil":
                    return "ID_Factura";
                case "Detalle_Factura_Quito":
                case "Detalle_Factura_Guayaquil":
                    return "ID_Factura";  // Parte de la clave primaria compuesta
                case "Producto":
                    return "ID_Producto";
                case "Inventario_Quito":
                case "Inventario_Guayaquil":
                    return "ID_Producto";  // Clave primaria en ambas tablas de inventario
                default:
                    return null;
            }
        }
    }
}
