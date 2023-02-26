using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using InventoryApp.Commands;
using InventoryApp.Models;
using System.Collections.Generic;


namespace InventoryApp.ViewModels
{
    public class AddItemParameters : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        private int quantity;
        public int Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<InventoryItem> _inventory;
        private InventoryItem _currentItem;

        public ObservableCollection<InventoryItem> Inventory
        {
            get { return _inventory; }
            set { _inventory = value; OnPropertyChanged("Inventory"); }
        }

        public InventoryItem CurrentItem
        {
            get { return _currentItem; }
            set { _currentItem = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand UpdateCommand { get; }

        public MainWindowViewModel()
        {
            Inventory = new ObservableCollection<InventoryItem>(GetInventory());

            AddCommand = new RelayCommand<AddItemParameters>(param => AddItem(param, this), CanAddItem);
            DeleteCommand = new RelayCommand<InventoryItem>(DeleteItem, CanDeleteItem);
            UpdateCommand = new RelayCommand<InventoryItem>(UpdateItem, CanUpdateItem);
        }

        private static IEnumerable<InventoryItem> GetInventory()
        {
            const string SelectSql = "SELECT * FROM Inventory";
            using (SQLiteConnection connection = GetConnection())
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(SelectSql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new InventoryItem
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Quantity = reader.GetInt32(3)
                            };
                        }
                    }
                }
            }
        }
        public AddItemParameters AddItemParametersInstance { get; set; } = new AddItemParameters();

        private static void AddItem(AddItemParameters parameters, MainWindowViewModel mainWindowViewModel)
        {
            const string InsertSql = "INSERT INTO Inventory (Name, Description, Quantity) VALUES (@Name, @Description, @Quantity)";
            using (SQLiteConnection connection = GetConnection())
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(InsertSql, connection))
                {
                    command.Parameters.AddWithValue("@Name", parameters.Name);
                    command.Parameters.AddWithValue("@Description", parameters.Description);
                    command.Parameters.AddWithValue("@Quantity", parameters.Quantity);
                    command.ExecuteNonQuery();
                }
            }
            mainWindowViewModel.Inventory.Add(new InventoryItem
            {
                ID = mainWindowViewModel.Inventory.Count + 1,
                Name = parameters.Name,
                Description = parameters.Description,
                Quantity = parameters.Quantity
            });
        }

        private static bool CanAddItem(object parameter)
        {
            return true;
        }
        private void DeleteItem(InventoryItem item)
        {
            const string DeleteSql = "DELETE FROM Inventory WHERE ID = @ID";
            using (SQLiteConnection connection = GetConnection())
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(DeleteSql, connection))
                {
                    command.Parameters.AddWithValue("@ID", item.ID);
                    command.ExecuteNonQuery();
                }
            }
            Inventory.Remove(item);
        }
        private static bool CanDeleteItem(InventoryItem item)
        {
            return item != null;
        }

        private static void UpdateItem(InventoryItem item)
        {
            const string UpdateSql = "UPDATE Inventory SET Name = @Name, Description = @Description, Quantity = @Quantity WHERE ID = @ID";
            using (SQLiteConnection connection = GetConnection())
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(UpdateSql, connection))
                {
                    command.Parameters.AddWithValue("@Name", item.Name);
                    command.Parameters.AddWithValue("@Description", item.Description);
                    command.Parameters.AddWithValue("@Quantity", item.Quantity);
                    command.Parameters.AddWithValue("@ID", item.ID);
                    command.ExecuteNonQuery();
                }
            }

        }

        private static bool CanUpdateItem(InventoryItem item)
        {
            return item != null;
        }

        private static SQLiteConnection GetConnection()
        {
            const string ConnectionString = "Data Source=Inventory.db";
            return new SQLiteConnection(ConnectionString);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}


