using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TiendaMaruja
{
    public partial class frmTotal : Form
    {
        private int idFactura;  // Id de la factura
        private string sucursalId; // ID de la sucursal (SUC001 o SUC002)
        private decimal totalFactura;  // Variable para almacenar el total de la factura

        // Constructor que recibe el Id de la factura y la sucursal
        public frmTotal(int idFactura, string sucursalId)
        {
            InitializeComponent();
            this.idFactura = idFactura;
            this.sucursalId = sucursalId;

            ConfigurarDataGridView();  // Configurar las columnas del DataGridView
            MostrarResumen();  // Mostrar el resumen de la factura en el formulario
        }

        // Método para configurar las columnas del DataGridView
        private void ConfigurarDataGridView()
        {
            // Asegúrate de que el DataGridView esté vacío antes de configurar
            dataGridView1.Columns.Clear();

            // Agregar las columnas al DataGridView
            dataGridView1.Columns.Add("Cliente", "Cliente");
            dataGridView1.Columns.Add("Empleado", "Empleado");
            dataGridView1.Columns.Add("Sucursal", "Sucursal");
            dataGridView1.Columns.Add("TotalFactura", "Total de la Factura");

            // Configurar el ajuste automático de columnas
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Método para mostrar el resumen de la factura en el formulario
        private void MostrarResumen()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    connection.Open();

                    // Seleccionar la tabla de factura según la sucursal
                    string tablaFactura = sucursalId == "QUITO" ? "Factura_Quito" : "Factura_Guayaquil";

                    // Consulta para obtener los datos de la factura
                    string query = $@"SELECT 
                                        c.Nombre_Cliente + ' ' + c.Apellido_Cliente AS Cliente,
                                        e.Nombre_Empleado + ' ' + e.Apellido_Empleado AS Empleado,
                                        s.Ciudad_Sucursal + ' - ' + s.Direccion_Sucursal AS Sucursal,
                                        f.Monto AS TotalFactura
                                    FROM {tablaFactura} f
                                    INNER JOIN Cliente c ON f.ID_Cliente = c.ID_Cliente
                                    INNER JOIN Empleado_Quito e ON f.ID_Empleado = e.ID_Empleado
                                    INNER JOIN Sucursal s ON f.ID_Sucursal = s.ID_Sucursal
                                    WHERE f.ID_Factura = @IdFac";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@IdFac", idFactura);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Agregar los datos de la factura al DataGridView
                        dataGridView1.Rows.Add(reader["Cliente"].ToString(),
                                               reader["Empleado"].ToString(),
                                               reader["Sucursal"].ToString(),
                                               Convert.ToDecimal(reader["TotalFactura"]).ToString("C2"));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al mostrar el resumen de la factura: " + ex.Message);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Se imprimió con éxito.", "Imprimir", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
