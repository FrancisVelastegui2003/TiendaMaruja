using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TiendaMaruja
{
    public partial class frmConsultas : Form
    {
        public frmConsultas()
        {
            InitializeComponent();
        }

        private void btnEmpleado_Click(object sender, EventArgs e)
        {
            frmEmpleados emp = new frmEmpleados(); 
            emp.Show();
            this.Hide(); 
        }

        private void btnCliente_Click(object sender, EventArgs e)
        {
            frmCRUD crud = new frmCRUD();
            crud.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmProducto prod = new frmProducto();
            prod.Show();
            this.Hide();
        }

        private void btnVenta_Click(object sender, EventArgs e)
        {
            frmVerificar frmVerificar = new frmVerificar();
            frmVerificar.Show();
            this.Hide();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Cerrar la aplicación
        }
    }
}
