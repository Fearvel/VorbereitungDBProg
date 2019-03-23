using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

// TABELLEN Person.Person, Person.BusinessEntityAddress, Person.Address aus AdventureWorks2014

namespace WpfApp1
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// Einfaches unbereinigtes, unoptimiertes Beispiel für die Nutzung von WPF und entity Framework ohne Binding
    /// </summary>
    public partial class MainWindow : Window
    {
        private AdventureWorks2014Entities _ctx; //Context

        private bool
            _suspressUpdate =
                true; // Scheneller aber unsauberer Workarround(der wird keine punkte abziehen). bei true keine update NonQuery

        public MainWindow()
        {
            InitializeComponent();
            _ctx = new AdventureWorks2014Entities();
            LoadListView1Items();
            ResetAddressDisplayArea();
        }

        /// <summary>
        /// Läd den rechten teil
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listV = ((ListView) sender); // Man kann auch ListView1 ansprechen Listview1 == sender
            if (listV.SelectedIndex >= 0)
            {
                Person p = (Person) listV.SelectedItem; // Das SelectedItem ist da Entity Framework eine Person
                if (_ctx.Database.Connection.State == ConnectionState.Closed)// Offnen der Verbindung wenn geschlossen
                {
                    _ctx.Database.Connection.Open();
                }

                SqlCommand com =
                    new SqlCommand(
                        "Select * from Person.Address where AddressID in " +
                        "(Select AddressID from Person.BusinessEntityAddress where BusinessEntityID = @BusinessEntityID)",
                        (SqlConnection) _ctx.Database.Connection);
                com.Parameters.AddWithValue("BusinessEntityID", p.BusinessEntityID); // Prepared Statements
                var da = new SqlDataAdapter(com);
                DataSet ds = new DataSet();
                da.Fill(ds);
                _ctx.Database.Connection.Close(); //schließen der Verbindung
                if (ds.Tables.Count > 0) //Es wird nur eine Datatable bei diesem SqlCommand erwartet
                {
                    _suspressUpdate = true;
                    var dt = ds.Tables[0]; //Die DataTable
                    if (dt.Rows.Count > 0)
                    {
                        var dr = dt.Rows[0]; //Die DataRow
                        LabelAddressID.Content = dr.Field<int>("AddressId"); //Fülle die Textboxen
                        TextBoxAddressLine1.Text = dr.Field<string>("AddressLine1"); //Fülle die Textboxen
                        TextBoxAddressLine2.Text = dr.Field<string>("AddressLine2"); //Fülle die Textboxen
                        TextBoxCity.Text = dr.Field<string>("City"); //Fülle die Textboxen
                        TextBoxPostalCode.Text = dr.Field<string>("PostalCode"); //Fülle die Textboxen

                        EnableAddressDisplayArea(); //Aktiviert die Textboxen
                    }
                }
            }

            _suspressUpdate = false; // Flag auf false
        }

        /// <summary>
        /// Läd den Listview
        /// der filter ist ein optionaler Parameter
        /// </summary>
        /// <param name="filter"></param>
        private void LoadListView1Items(string filter = "")
        {
            ResetAddressDisplayArea(); //Sperren, da ListView 1 sich verändert
            var person = _ctx.Person.ToList();
            ListView1.ItemsSource = person.Where(
                x => (x.FirstName + " " + x.LastName).ToLower().Contains(filter.ToLower()));//Einfache nutzung des Filters
            // damit das funktioniert muss in der Person.cs dies ergänzt werden, da die ListView ToString zur Anzeige aufruft.
            //public override string ToString()
            //{
            //    return FirstName + " " + LastName;
            //}
        }


        /// <summary>
        /// Updated eine DB eintrag
        /// </summary>
        private void Update()
        {
            if (!_suspressUpdate)
            {
                if (_ctx.Database.Connection.State == ConnectionState.Closed) // Offnen der Verbindung wenn geschlossen
                {
                    _ctx.Database.Connection.Open();
                }

                SqlCommand com =
                    new SqlCommand(
                        "Update Person.Address set AddressLine1=@AddressLine1 , AddressLine2=@AddressLine2, " +
                        "City=@City, PostalCode=@PostalCode where AddressId =@Id",
                        (SqlConnection) _ctx.Database.Connection);


                com.Parameters.AddWithValue("@Id", int.Parse(LabelAddressID.Content.ToString()));

                com.Parameters.AddWithValue("@AddressLine1", TextBoxAddressLine1.Text);
                com.Parameters.AddWithValue("@AddressLine2", TextBoxAddressLine2.Text);
                com.Parameters.AddWithValue("@City", TextBoxCity.Text);
                com.Parameters.AddWithValue("@PostalCode", TextBoxPostalCode.Text);

                com.ExecuteNonQuery();
                _ctx.Database.Connection.Close(); //schließen der Verbindung

            }
        }

        /// <summary>
        /// Alle Textboxen außer der Suche rufen das auf wenn der text in einer der Boxen sich verändert
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e"></param>
        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Update();
        }

        /// <summary>
        /// Entsperrt die Textboxen
        /// </summary>
        private void EnableAddressDisplayArea()
        {
            TextBoxAddressLine1.IsEnabled = true;
            TextBoxAddressLine2.IsEnabled = true;
            TextBoxCity.IsEnabled = true;
            TextBoxPostalCode.IsEnabled = true;
        }

        /// <summary>
        /// Leert und sperrt die TextBoxen
        /// </summary>
        private void ResetAddressDisplayArea()
        {
            _suspressUpdate = true;
            LabelAddressID.Content = "";

            TextBoxAddressLine1.Clear();
            TextBoxAddressLine2.Clear();
            TextBoxCity.Clear();
            TextBoxPostalCode.Clear();
            TextBoxAddressLine1.IsEnabled = false;
            TextBoxAddressLine2.IsEnabled = false;
            TextBoxCity.IsEnabled = false;
            TextBoxPostalCode.IsEnabled = false;
            _suspressUpdate = false;
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LoadListView1Items(((TextBox) sender).Text);
        }
    }
}