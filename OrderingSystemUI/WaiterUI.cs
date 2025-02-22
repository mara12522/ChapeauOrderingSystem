﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OrderingSystemLogic;
using OrderingSystemModel;

namespace OrderingSystemUI
{
    public partial class WaiterUI : Form
    {
        //create services
        OrderProductService orderProductService = new OrderProductService();
        ProductService productService = new ProductService();
        TableService tableService = new TableService();
        OrderService orderService = new OrderService();
        PaymentService paymentService = new PaymentService();

        //creating all kinds of Object which are needed
        List<Product> addingNewOrdersList;
        List<int> alreadyPrinted = new List<int>();
        List<Product> lunchProducts;
        List<Product> dinnerProducts;
        List<Product> drinkProducts;
        int temporaryCountVariable = 0;

        Table currentTable = null;
        Employee currentEmployee = null;
        Order currentOrder = null;
        OrderProduct currentOrderItem;

        Product selectedProduct = null;
        Product selectedProductsOnAddList = null;
        Payment currentPayment = null;

        Button tableButton = null;
        public WaiterUI(Employee currentEmployee)
        {
            InitializeComponent();
            this.currentEmployee = currentEmployee;
        }

        private void WaiterUI_Load(object sender, EventArgs e)
        {
            try
            {
                //Disables tabs in tabcontrol
                pnlOrderingSystem.Appearance = TabAppearance.FlatButtons;
                pnlOrderingSystem.ItemSize = new Size(0, 1);
                pnlOrderingSystem.SizeMode = TabSizeMode.Fixed;

                currentOrderItem = null;
                currentPayment = new Payment();
                addingNewOrdersList = new List<Product>();

                SetTablesColor();
                HideButtons();
                SetOrderDisplay();


                //displaying the logged in Employee in the upper right corner
                btnEmployeeName.Text = currentEmployee.EmployeeName;

                pnlOrderingSystem.SelectedTab = tableViewTabCommentQ;

                pnlTableStatus.Hide();
                pnlAddComment.Hide();
                lblTableNumber.Text = "";

                //creating lists with the Products on the menu, data from DB
                lunchProducts = productService.GetProductByCategory(1);
                dinnerProducts = productService.GetProductByCategory(2);
                drinkProducts = productService.GetProductByCategory(3);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        //After choosing a table to serve
        //Displaying all items in the current order
        private void OrderOverview()
        {
            lblTableNumber.Show();
            pnlOrderingSystem.SelectedTab = tableOrderOverviewTab;
            btnRemove.Hide();
            btnEdit.Hide();
            btnConfirm.Hide();
            txtboxEdit.Hide();
            lblNewStock.Hide();
            addingNewOrdersList = new List<Product>();
            try
            {
                listViewTableOrder.Items.Clear();

                List<OrderProduct> products = orderProductService.GetAllOrderProducts();

                List<Product> productsForThisOrder = new List<Product>();
                List<int> alreadyPrinted = new List<int>();

                foreach (OrderProduct orderProduct in products)
                {
                    Order order = orderService.GetOrder(currentOrder.OrderNumber);
                    Product product = productService.GetProduct(orderProduct.ProductID);

                    if (orderProduct.OrderNumber == currentOrder.OrderNumber)
                    {
                        product.Status = orderProduct.Status;
                        productsForThisOrder.Add(product);
                    }

                }
                DisplayItemsWithCount(productsForThisOrder);

            }
            catch (Exception e)
            {
                MessageBox.Show("Error occured: ", e.Message);
            }
        }
        private float CalculateTotal(float total, Product product)
        {
            return total + product.Price;
        }
        private void DisplayItemsWithCount(List<Product> productsForThisOrder)
        {
            List<int> alreadyPrinted = new List<int>();

            float total = 0;
            foreach (Product product in productsForThisOrder)
            {
                total = CalculateTotal(total, product);
                int count = 0;
                foreach (Product p in productsForThisOrder)
                {
                    if (p.ProductID == product.ProductID)
                    {
                        count++;
                    }
                }

                if (!alreadyPrinted.Contains(product.ProductID))
                {
                    ListViewItem item = new ListViewItem();

                    item.Text = $"{count} x"; //count
                    item.SubItems.Add(product.ProductName);//product name
                    item.SubItems.Add($"€ {product.Price.ToString("0.00")}");//price
                    item.Tag = product;
                    listViewTableOrder.Items.Add(item);
                }
                if (count > 1)
                    alreadyPrinted.Add(product.ProductID);
            }
            lblDisplayTotal.Text = "€ " + total.ToString("0.00");
        }

        //go back to the TableOverview
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to discontinue the current process and go back to the table overview?", $"Going to table view", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                currentTable = null;
                addingNewOrdersList = null;
                listViewOrderSummary.Items.Clear();
                tableNumber.Text = "";
                pnlTableStatus.Hide();
                pnlOrderingSystem.SelectedTab = tableViewTabCommentQ;
            }
            else
            {
                return;
            }
        }

        //giving the expected part of the menu from clicking the buttons
        private void btnLunch_Click(object sender, EventArgs e)
        {
            LunchMenuDisplay();
        }
        private void btnDinner_Click(object sender, EventArgs e)
        {
            DinnerMenuDisplay();

        }
        private void btnDrinks_Click(object sender, EventArgs e)
        {
            DrinksMenuDisplay();
        }
        private void btnDinnerMenu_Click(object sender, EventArgs e)
        {
            DinnerMenuDisplay();
        }
        private void btnLunchMenu_Click(object sender, EventArgs e)
        {
            LunchMenuDisplay();
        }
        private void btnDrinksMenu_Click(object sender, EventArgs e)
        {
            DrinksMenuDisplay();
        }

        //Lunch Menu after klicking the LUNCH button
        private void LunchMenuDisplay()
        {
            pnlOrderingSystem.SelectedTab = addOrderView;
            listViewAddOrder.Items.Clear();
            listViewAddOrder.View = View.Tile;
            btnComment.Hide();
            btnRemoveNew.Hide();

            //List groups
            ListViewGroup lunchStarters = new ListViewGroup("Starters", HorizontalAlignment.Center);
            ListViewGroup lunchMains = new ListViewGroup("Mains", HorizontalAlignment.Center);
            ListViewGroup lunchDeserts = new ListViewGroup("Deserts", HorizontalAlignment.Center);
            try
            {
                foreach (Product product in lunchProducts)
                {
                    ListViewGroup group;
                    if (product.ProductType == "starters")
                        group = lunchStarters;
                    else if (product.ProductType == "mains")
                        group = lunchMains;
                    else if (product.ProductType == "deserts")
                        group = lunchDeserts;
                    else throw new ArgumentException("Mistake in the database with the menu");

                    ListViewItem item = new ListViewItem(product.ProductName, group);
                    item.Tag = product;
                    //item.Text = product.ProductName;
                    //item.SubItems.Add($"€ {product.Price.ToString("0.00")}");//price

                    listViewAddOrder.Items.Add(item);

                }
                listViewAddOrder.Groups.Add(lunchStarters);
                listViewAddOrder.Groups.Add(lunchMains);
                listViewAddOrder.Groups.Add(lunchDeserts);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }
        //Dinner Menu after klicking the DINNER button
        private void DinnerMenuDisplay()
        {
            listViewAddOrder.Clear();

            pnlOrderingSystem.SelectedTab = addOrderView;
            listViewAddOrder.Items.Clear();
            listViewAddOrder.View = View.Tile;
            btnComment.Hide();
            btnRemoveNew.Hide();

            //List groups
            ListViewGroup dinnerStarters = new ListViewGroup("Starters", HorizontalAlignment.Center);
            ListViewGroup dinnerEntres = new ListViewGroup("Entres", HorizontalAlignment.Center);
            ListViewGroup dinnerMains = new ListViewGroup("Mains", HorizontalAlignment.Center);
            ListViewGroup dinnerDeserts = new ListViewGroup("Deserts", HorizontalAlignment.Center);
            try
            {

                foreach (Product product in dinnerProducts)
                {
                    ListViewGroup group;
                    if (product.ProductType == "starters")
                        group = dinnerStarters;
                    else if (product.ProductType == "entrements")
                        group = dinnerEntres;
                    else if (product.ProductType == "mains")
                        group = dinnerMains;
                    else if (product.ProductType == "deserts")
                        group = dinnerDeserts;
                    else throw new ArgumentException("Mistake in the database with the menu");

                    ListViewItem item = new ListViewItem(product.ProductName, group);
                    //item.Text = product.ProductName;
                    //item.SubItems.Add($"€ {product.Price.ToString("0.00")}");//price
                    item.Tag = product;

                    listViewAddOrder.Items.Add(item);

                }
                listViewAddOrder.Groups.Add(dinnerStarters);
                listViewAddOrder.Groups.Add(dinnerEntres);
                listViewAddOrder.Groups.Add(dinnerMains);
                listViewAddOrder.Groups.Add(dinnerDeserts);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }
        //Drink Menu after klicking the DRINK button
        private void DrinksMenuDisplay()
        {
            listViewAddOrder.Clear();

            pnlOrderingSystem.SelectedTab = addOrderView;
            listViewAddOrder.Items.Clear();
            listViewAddOrder.View = View.Tile;
            btnComment.Hide();
            btnRemoveNew.Hide();

            //List groups
            ListViewGroup softDrinks = new ListViewGroup("Soft Drinks", HorizontalAlignment.Center);
            ListViewGroup beers = new ListViewGroup("Beers", HorizontalAlignment.Center);
            ListViewGroup wines = new ListViewGroup("Wines", HorizontalAlignment.Center);
            ListViewGroup spirits = new ListViewGroup("Spirits", HorizontalAlignment.Center);
            ListViewGroup hotDrinks = new ListViewGroup("Hot Drinks", HorizontalAlignment.Center);

            try
            {
                foreach (Product product in drinkProducts)
                {
                    ListViewGroup group;
                    if (product.ProductType == "soft drinks")
                        group = softDrinks;
                    else if (product.ProductType == "beers")
                        group = beers;
                    else if (product.ProductType == "wines")
                        group = wines;
                    else if (product.ProductType == "spirits")
                        group = spirits;
                    else if (product.ProductType == "coffe/tea")
                        group = hotDrinks;
                    else throw new ArgumentException("Mistake in the database with the menu");

                    ListViewItem item = new ListViewItem(product.ProductName, group);
                    //item.Text = product.ProductName;
                    //item.SubItems.Add($"€ {product.Price.ToString("0.00")}");//price
                    item.Tag = product;

                    listViewAddOrder.Items.Add(item);

                }
                listViewAddOrder.Groups.Add(softDrinks);
                listViewAddOrder.Groups.Add(beers);
                listViewAddOrder.Groups.Add(wines);
                listViewAddOrder.Groups.Add(spirits);
                listViewAddOrder.Groups.Add(hotDrinks);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }

        private void btnTable1_Click(object sender, EventArgs e)
        {
            ButtonsClick(1);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            ChangeStatus(currentTable.TableNumber);
            pnlTableStatus.Hide();
            RBfree.Checked = false;
            RBoccupied.Checked = false;
            RBreserved.Checked = false;
        }

        public void ChangeStatus(int tableNumber)
        {
            lblTableNumber.Text = tableNumber.ToString();
            Table table = tableService.GetTable(tableNumber);
            string tableStatus;


            if (RBfree.Checked)
            {
                tableStatus = "free";
                tableService.ChangeTableStatus(tableNumber, tableStatus);
            }
            else if (RBoccupied.Checked)
            {
                tableStatus = "occupied";
                tableService.ChangeTableStatus(tableNumber, tableStatus);


                //code for creating new order every time a table is newly occupied so a new group of people can order with that ordernumber
                Order newOrder = new Order();
                newOrder.EmployeeNumber = currentEmployee.EmployeNumber;
                newOrder.OrderTime = DateTime.Now;
                orderService.CreateNewOrder(newOrder);
                int newCurrentOrder = orderService.GetOrderNumber().OrderNumber;
                currentTable.CurrentOrder = newCurrentOrder;
                tableService.UpdateTableWithCurrentOrder(currentTable, currentTable.CurrentOrder);
            }
            else if (RBreserved.Checked)
            {
                tableStatus = "reserved";
                tableService.ChangeTableStatus(tableNumber, tableStatus);
            }
            if (currentTable.TableStatus != "occupied")
                btnGoToTable.Enabled = false;

            SetTablesColor();
        }

        public void SetTablesColor()
        {
            TableColor(tableService.GetTable(1), btnTable1);
            TableColor(tableService.GetTable(2), btnTable2);
            TableColor(tableService.GetTable(3), btnTable3);
            TableColor(tableService.GetTable(4), btnTable4);
            TableColor(tableService.GetTable(5), btnTable5);
            TableColor(tableService.GetTable(6), btnTable6);
            TableColor(tableService.GetTable(7), btnTable7);
            TableColor(tableService.GetTable(8), btnTable8);
            TableColor(tableService.GetTable(9), btnTable9);
            TableColor(tableService.GetTable(10), btnTable10);
        }

        public void SetOrderButton(Table table, Button btn1, Button btn2)
        {
            table = tableService.GetTable(table.TableNumber);
            List<OrderProduct> drinProducts = orderProductService.GetOrderProductsDrink(table.CurrentOrder);
            if (table.TableStatus == "occupied")
            {
                if (drinProducts.Count > 0)
                {
                    foreach (OrderProduct product in orderProductService.GetOrderProductsDrink(table.CurrentOrder))
                    {
                        if (product.Status == "prepared")
                        {
                            btn1.BackColor = Color.Gold;
                            btn1.Show();
                            break;
                        }
                        else
                        {
                            btn1.Show();
                        }
                    }
                }
            }

            List<OrderProduct> fooodProducts1 = orderProductService.GetOrderProductsFood(table.CurrentOrder);

            if (table.TableStatus == "occupied")
            {
                if (fooodProducts1.Count > 0)
                {
                    foreach (OrderProduct product in orderProductService.GetOrderProductsFood(table.CurrentOrder))
                    {
                        if (product.Status == "prepared")
                        {
                            btn2.BackColor = Color.Gold;
                            btn2.Show();
                            break;
                        }
                        else
                        {
                            btn2.Show();
                        }
                    }
                }
            }
        }

        public void SetOrderDisplay()
        {
            SetOrderButton(tableService.GetTable(1), btnD1, btnF1);
            SetOrderButton(tableService.GetTable(2), btnD2, btnF2);
            SetOrderButton(tableService.GetTable(3), btnD3, btnF3);
            SetOrderButton(tableService.GetTable(4), btnD4, btnF4);
            SetOrderButton(tableService.GetTable(5), btnD5, btnF5);
            SetOrderButton(tableService.GetTable(6), btnD6, btnF6);
            SetOrderButton(tableService.GetTable(7), btnD7, btnF7);
            SetOrderButton(tableService.GetTable(8), btnD8, btnF8);
            SetOrderButton(tableService.GetTable(9), btnD9, btnF9);
            SetOrderButton(tableService.GetTable(10), btnD10, btnF10);

        }

        public void TableColor(Table table, Button btn)
        {
            if (table.TableStatus == "free")
                btn.BackColor = Color.PaleGreen;
            else if (table.TableStatus == "occupied")
                btn.BackColor = Color.FromArgb(255, 128, 0);
            else if (table.TableStatus == "reserved")
                btn.BackColor = Color.DarkGray;
        }
        public void ButtonsClick(int tableNumber)
        {
            pnlTableStatus.Show();
            currentTable = tableService.GetTable(tableNumber);
            if (currentTable.TableStatus != "occupied")
                btnGoToTable.Enabled = false;
            else btnGoToTable.Enabled = true;
            currentOrder = orderService.GetOrder(currentTable.CurrentOrder);
            lblTableNumber.Text = "TABLE " + currentTable.TableNumber;
        }
        private void btnGoToTable_Click(object sender, EventArgs e)
        {
            ChangeStatus(currentTable.TableNumber);
            tableNumber.Text = "TABLE " + currentTable.TableNumber;
            currentOrder = orderService.GetOrder(currentTable.CurrentOrder);
            OrderOverview();
            RBfree.Checked = false;
            RBoccupied.Checked = false;
            RBreserved.Checked = false;
        }

        private void btnTable2_Click(object sender, EventArgs e)
        {
            ButtonsClick(2);
        }

        private void btnTable3_Click(object sender, EventArgs e)
        {
            ButtonsClick(3);
        }

        private void btnTable4_Click(object sender, EventArgs e)
        {
            ButtonsClick(4);
        }

        private void btnTable5_Click(object sender, EventArgs e)
        {
            ButtonsClick(5);
        }

        private void btnTable6_Click(object sender, EventArgs e)
        {
            ButtonsClick(6);
        }

        private void btnTable7_Click(object sender, EventArgs e)
        {
            ButtonsClick(7);
        }

        private void btnTable8_Click(object sender, EventArgs e)
        {
            ButtonsClick(8);
        }

        private void btnTable9_Click(object sender, EventArgs e)
        {
            ButtonsClick(9);
        }

        private void btnTable10_Click(object sender, EventArgs e)
        {
            ButtonsClick(10);
        }

        private void btnEmployeeName_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", $"See you soon {currentEmployee.EmployeeName}", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                //log out and go back to login form
                //?currentEmployee == null;
                Close();
                LoginForm loginForm = new LoginForm();
                loginForm.ShowDialog();  
            }
            else
            {
                return;
            }
        }
        //selecting products from the menu to add to a table order
        private void listViewAddOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listViewAddOrder.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listViewAddOrder.SelectedItems[0];
                    selectedProduct = (Product)selectedItem.Tag;
                    //check if item is available
                    if (selectedProduct.Stock <= 0)
                    {
                        MessageBox.Show("Product is out of Stock please choose something else!");
                    }
                    else
                    {
                        addingNewOrdersList.Add(selectedProduct);
                    }

                    DisplaySelectedItemsForOrder();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }
        //overview over the items that will be added to the table order
        private void DisplaySelectedItemsForOrder()
        {
            listViewOrderSummary.Items.Clear();

            alreadyPrinted = new List<int>();

            try
            {
                foreach (Product product in addingNewOrdersList)
                {
                    int count = 0;
                    foreach (Product p in addingNewOrdersList)
                    {
                        if (p.ProductID == product.ProductID)
                        {
                            count++;
                        }
                    }

                    ListViewItem item = new ListViewItem();

                    if (!alreadyPrinted.Contains(product.ProductID))
                    {
                        item.Text = $"{count} x"; //count
                        item.SubItems.Add(product.ProductName);//product name
                        if (product.TemporaryComment != null)
                            item.SubItems.Add(product.TemporaryComment.ToString());
                        listViewOrderSummary.Items.Add(item);
                    }
                    item.Tag = product;
                    alreadyPrinted.Add(product.ProductID);
                    listViewAddOrder.SelectedItems.Clear();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }

        //adding selected items to the table order and DB orderProduct
        private void btnAddOrder_Click(object sender, EventArgs e)
        {
            try
            {
                //add the created list of orderProducts to the actual order
                if (addingNewOrdersList.Count > 0)
                {
                    for (int i = 0; i < addingNewOrdersList.Count; i++)
                    {
                        if (addingNewOrdersList[i].TemporaryComment == null)
                            addingNewOrdersList[i].TemporaryComment = "";
                        orderProductService.AddOrderItem(currentOrder.OrderNumber, addingNewOrdersList[i].ProductID, addingNewOrdersList[i].TemporaryComment, addingNewOrdersList[i].ProductCategory);
                        //edit stock in db
                        int newStock = 0;
                        newStock = addingNewOrdersList[i].Stock - 1;
                        productService.EditStock(addingNewOrdersList[i].ProductID, newStock);
                    }
                    addingNewOrdersList = null;
                }
                listViewOrderSummary.Items.Clear();
                OrderOverview();
            }catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                //remove one item in the list
                DialogResult result = MessageBox.Show("Are you sure you want to remove this/these order Item(s)?", $"Item(s) removed", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    orderProductService.RemoveOrderItem(currentOrderItem.ProductID, currentOrderItem.OrderNumber);
                    OrderOverview();
                }
                else
                {
                    return;
                }
                listViewTableOrder.Refresh();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            txtboxEdit.Show();
            btnConfirm.Show();
            lblNewStock.Show();
        }

        private void listViewTableOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                btnRemove.Show();
                btnEdit.Show();
                if (listViewTableOrder.SelectedItems.Count > 0)
                {

                    ListViewItem selectedItem = listViewTableOrder.SelectedItems[0];
                    string[] split = selectedItem.Text.Split(" ");
                    txtboxEdit.Text = split[0];
                    temporaryCountVariable = int.Parse(split[0]);
                    Product currentProduct = (Product)selectedItem.Tag;

                    currentOrderItem = orderProductService.GetOrderProduct(currentOrder.OrderNumber, currentProduct.ProductID);

                    if (listViewTableOrder.SelectedItems.Count > 1) btnEdit.Hide();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }



        }

        //PAYMENT UI

        //method for displaying list of orders & price & VAT
        private void PaymentOrderOverview()
        {
            pnlOrderingSystem.SelectedTab = tabPagePayment;
           
            try
            {
                listViewPaymentOrder.Items.Clear();

                List<OrderProduct> products = orderProductService.GetAllOrderProducts();

                List<Product> productsForThisOrder = new List<Product>();
                List<int> alreadyPrinted = new List<int>();


                foreach (OrderProduct orderProduct in products)
                {
                    Order order = orderService.GetOrder(currentOrder.OrderNumber);
                    Product product = productService.GetProduct(orderProduct.ProductID);

                    if (orderProduct.OrderNumber == currentOrder.OrderNumber)
                    {
                        productsForThisOrder.Add(product);
                    }
                }
                float total = 0;
                float vat = 0;
              

                foreach (Product product in productsForThisOrder)
                {
                    total += product.Price;
                    vat = vat + product.Price * product.VAT;
                   
                    int count = 0;
                    foreach (Product p in productsForThisOrder)
                    {
                        if (p.ProductID == product.ProductID)
                        {
                            count++;
                        }
                    }

                    if (!alreadyPrinted.Contains(product.ProductID))
                    {
                        ListViewItem item = new ListViewItem();
                        item.Text = $"{count} x"; //count
                        item.SubItems.Add(product.ProductName);//product name
                        item.SubItems.Add($"€ {product.Price.ToString("0.00")}");//price

                        listViewPaymentOrder.Items.Add(item);
                    }
                    if (count > 1)
                        alreadyPrinted.Add(product.ProductID);

                }
                //storing total and vat to database
                currentPayment.PaymentAmount = total;
                currentPayment.PaymentVat = vat;
                
                //display price
                lbl_price1.Text = "€ " + total.ToString("0.00");
                lbl_price2.Text = "€ " + total.ToString("0.00");
                lbl_price3.Text = "€ " + total.ToString("0.00");
                lbl_price4.Text = "€ " + total.ToString("0.00");
                lbl_pricePartly.Text = "€ " + total.ToString("0.00");

                //display vat
                lbl_vat_amount.Text = vat.ToString("0.00");
                lbl_vat_amount2.Text = vat.ToString("0.00");
                

            }
            catch (Exception e)
            {
                MessageBox.Show("Error occured: ", e.Message);
            }
        }
        //CALCULATIONS
        private void CalculateChange()
        {
           //convert price to price without euro sign
           string lblWithoutEuro = lbl_price3.Text.Substring(lbl_price3.Text.LastIndexOf('€') + 1);
           float price = float.Parse(lblWithoutEuro);

            if (String.IsNullOrEmpty(txtBox_amountPaid.Text))
            {
                MessageBox.Show("Please, check entered amount again");
                return;
            }

           float amountPaid = float.Parse(txtBox_amountPaid.Text);

           float change = amountPaid - price;
           lbl_change.Text = "€ " + change.ToString("0.00");
           
            //check if the amount paid is entered correctly, can't be smaller than order price

            if (amountPaid < price)
            {
            lbl_change.Hide();
            MessageBox.Show("Please, check entered amount again");
            return;
            }
            else lbl_change.Show();
        }
        private void CalculateChangeCustomisedTip()
        {
            //changing change amount based on entered tip

            string changeWithoutEuro = lbl_change.Text.Substring(lbl_change.Text.LastIndexOf('€') + 1);
            float change = float.Parse(changeWithoutEuro);

            string tipWithoutEuro = txtBox_CustomTip.Text.Substring(txtBox_CustomTip.Text.LastIndexOf('€') + 1);
            float customTip = float.Parse(tipWithoutEuro);
            

            float newChange = change - customTip;
            lbl_change.Text = "€ " + newChange.ToString("0.00");
            lbl_billChange.Text = "€ " + newChange.ToString("0.00");

        }

        /*private void CalculateTotalAmount()
        {
            //calculate amount paid when paying partly
            string paidCashWithoutEuro = txtBox_Cash.Text.Substring(txtBox_Cash.Text.LastIndexOf('€') + 1);
            int paidCash = int.Parse(paidCashWithoutEuro);

            string paidCardWithoutEuro = txtBox_cardAmount.Text.Substring(txtBox_cardAmount.Text.LastIndexOf('€') + 1);
            int paidCard = int.Parse(paidCardWithoutEuro);

            int totalAmount = paidCard + paidCash;
            if (txtBox_Cash.Text != " ")
            {
                lbl_amount_paid.Text = "€ " + totalAmount.ToString("0.00");
            }
            else
            {
                lbl_amount_paid.Text = txtBox_amountPaid.Text;
            }
        }
        */

            //navigate to Payment UI
            private void btnPay_Click(object sender, EventArgs e)
        {
            tableOrderOverviewTab.Hide();
            tabPagePayment.Show();
            PaymentOrderOverview();
        }
        private void radioBtn_DEBIT_CheckedChanged(object sender, EventArgs e)
        {
            
        }    
        //navigate to "PAYMENT OVERVIEW"
        private void btn_payment_Click(object sender, EventArgs e)
        {
           
          if (!radioBtn_CASH.Checked && !radioBtn_DEBIT.Checked && !radioBtn_VISA.Checked)
          {
            MessageBox.Show("To continue, please select payment type");
            return;
          }
            
            lbl_HasBeenAdded.Hide();
            tabPagePayment.Hide();
            tabPagePaymentView.Show();
        }

        private void btn_SetAmountPaid_Click(object sender, EventArgs e)
        {
            CalculateChange();
        }
        //navigate to "ANY COMMENTS?" page
        private void btn_Pay_Click(object sender, EventArgs e)
        {
           if (txtBox_amountPaid.Text == "")
           {
                MessageBox.Show("To continue the payment, you must fill in the amount paid by the customer!");
                return;
           }
              
            tabPagePaymentView.Hide();
            tabPageAnyComments.Show();
        }
        private void lbl9_Click(object sender, EventArgs e)
        {

        }
        //actions for "ANY COMMENTS?"
        private void btn_AddComment_Click(object sender, EventArgs e)
        {
            string comment = txtboxComment.Text;
            
            tabPageAnyComments.Hide();
            tabPageCustomerComment.Show();
        }
        //navigate to "SETTLE THE BILL" page
        private void btn_cntinuePayment_Click(object sender, EventArgs e)
        {
            currentPayment.CustomerComment = "";

            //display tip and amount paid

            if (txtBox_tipPartly.Text == "")
            {
                lbl_tip2.Text = lbl_tip3.Text;
                lbl_amount_paid.Text = "€ " + txtBox_amountPaid.Text;
            }
            else
            {
                lbl_tip2.Text = lbl_tipPartlyAdded.Text;
                int cashPaid = int.Parse(txtBox_Cash.Text);
                int cardPaid = int.Parse(txtBox_cardAmount.Text);
                int amountPartly = cardPaid + cashPaid;
                lbl_amount_paid.Text = "€ " + amountPartly.ToString("0.00");

            }

            
           // CalculateTotalAmount();
            
            SetPaymentType();
            tabPageAnyComments.Hide();
            tabPageSettledBill.Show();
        }
        private void SetPaymentType()
        {
            //add payment type + storing to database
            if (radioBtn_CASH.Checked)
            {
                lbl_paymentType.Text = "CASH";
                currentPayment.PaymentType = "cash";
            }
            if (radioBtn_DEBIT.Checked)
            {
                lbl_paymentType.Text = "DEBIT";
                currentPayment.PaymentType = "debit";
            }
            if (radiobtn_DEBITpartly.Checked)
            {
                lbl_paymentType.Text = "DEBIT and CASH";
                currentPayment.PaymentType = "cash";
                currentPayment.PaymentType = "debit";
            }
            if (radiobtn_VISApartly.Checked)
            {
                lbl_paymentType.Text = "VISA and CASH";
                currentPayment.PaymentType = "cash";
                currentPayment.PaymentType = "visa/amex";
            }
            else if (radioBtn_VISA.Checked)
            {
                lbl_paymentType.Text = "VISA/AMEX";
                currentPayment.PaymentType = "visa/amex";
            }
        }


        //navigate to "SETTLE THE BILL" page
        private void btn_Confirm_Click(object sender, EventArgs e)
        {
            //display tip and amount paid
           
            if (txtBox_tipPartly.Text == "")
            {
                lbl_tip2.Text = lbl_tip3.Text;
                lbl_amount_paid.Text = "€ " + txtBox_amountPaid.Text;
            }
            else
            {
                lbl_tip2.Text = lbl_tipPartlyAdded.Text;
                int cashPaid = int.Parse(txtBox_Cash.Text);
                int cardPaid = int.Parse(txtBox_cardAmount.Text);
                int amountPartly = cardPaid + cashPaid;
                lbl_amount_paid.Text = "€ " + amountPartly.ToString("0.00");
            }

            //CalculateTotalAmount();

            if (txtBox_Comment.Text == "")
            {
               MessageBox.Show("To continue, please fill in the comment section!");
               return;
            }
            else
            {
                currentPayment.CustomerComment = txtBox_Comment.Text;
            }

            SetPaymentType();
            tabPageCustomerComment.Hide();
            tabPageSettledBill.Show();
        }
           
        //show label "TIP HAS BEEN ADDED" after adding change as a tip
        private void btn_changeAsTip_Click(object sender, EventArgs e)
        {
            lbl_tip3.Text = lbl_change.Text;
            //if change is added as a tip, then change will always be 0
            lbl_billChange.Text = "-";
            
            //adding tip to database
            string tip = lbl_change.Text;
            string[] split = tip.Split(" ");
            currentPayment.Tip = float.Parse(split[1]);

            //changing change amount when customised tip has been added
            lbl_change.Text = "-";

            lbl_HasBeenAdded.Show();
            btn_changeAsTip.Hide();
            lbl5.Hide();
            txtBox_CustomTip.Hide();
            btn_SetTip.Hide();
        }
        private void label_HasBeenAdded_Click(object sender, EventArgs e)
        {

        }
        //show label "TIP HAS BEEN ADDED" setting a customized tip
        private void btn_SetTip_Click(object sender, EventArgs e)
        {
            //display tip
            int customTip = int.Parse(txtBox_CustomTip.Text);
            lbl_tip3.Text = "€ " + customTip.ToString("0.00");


            //adding tip to database
            currentPayment.Tip = customTip;

            CalculateChangeCustomisedTip();
            lbl_HasBeenAdded.Show();
            btn_changeAsTip.Hide();
            lbl5.Hide();
            txtBox_CustomTip.Hide();
            btn_SetTip.Hide();
        }
        //go back from "COMMENTS" to "SETTLE THE BILL"
        private void btn_back_Click(object sender, EventArgs e)
        {
            tabPageCustomerComment.Hide();
            tabPagePaymentView.Show();
        }

        private void listViewPaymentOrder_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void txtBox_Comment_TextChanged(object sender, EventArgs e)
        {

        }
        private void listViewOrderSummary_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnComment.Show();
            btnRemoveNew.Show();

            try
            {
                if (listViewOrderSummary.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listViewOrderSummary.SelectedItems[0];
                    selectedProductsOnAddList = (Product)selectedItem.Tag;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        private void btnRemoveNew_Click(object sender, EventArgs e)
        {
            addingNewOrdersList.Remove(selectedProductsOnAddList);
            DisplaySelectedItemsForOrder();
        }
        private void btnComment_Click_1(object sender, EventArgs e)
        {
            pnlAddComment.Show();
        }
        private void btnpnlAddComment_Click(object sender, EventArgs e)
        {
            string comment = txtboxComment.Text;
            selectedProductsOnAddList.TemporaryComment = comment;
            pnlAddComment.Hide();
            listViewOrderSummary.Items.Clear();
            DisplaySelectedItemsForOrder();
        }
        private void btnGoBack_Click(object sender, EventArgs e)
        {
            pnlAddComment.Hide();
        }
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                int newCount = int.Parse(txtboxEdit.Text);

                if (newCount > temporaryCountVariable)
                {
                    int itemsToAdd = newCount - temporaryCountVariable;
                    for (int i = 0; i < itemsToAdd; i++)
                    {
                        orderProductService.AddOrderItem(currentOrder.OrderNumber, currentOrderItem.ProductID, "", currentOrderItem.ProductCategory);
                    }
                }
                else if (newCount < temporaryCountVariable)
                {
                    int itemsToRemove = temporaryCountVariable - newCount;

                    for (int i = 0; i < itemsToRemove; i++)
                    {
                        OrderProduct productToRemove = orderProductService.GetOrderProduct(currentOrder.OrderNumber, currentOrderItem.ProductID);
                        orderProductService.RemoveOneOrderItem(productToRemove.ItemID);
                    }
                }
                temporaryCountVariable = 0;
                listViewTableOrder.SelectedItems.Clear();
                btnRemove.Hide();
                btnEdit.Hide();
                btnConfirm.Hide();
                txtboxEdit.Hide();

                OrderOverview();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public void HideButtons()
        {
            btnD1.Hide(); btnF1.Hide();
            btnD2.Hide(); btnF2.Hide();
            btnD3.Hide(); btnF3.Hide();
            btnD4.Hide(); btnF4.Hide();
            btnD5.Hide(); btnF5.Hide();
            btnD6.Hide(); btnF6.Hide();
            btnD7.Hide(); btnF7.Hide();
            btnD8.Hide(); btnF8.Hide();
            btnD9.Hide(); btnF9.Hide();
            btnD10.Hide(); btnF10.Hide();

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            SetTablesColor();
            HideButtons();
            SetOrderDisplay();
        }

        private void RBoccupied_CheckedChanged(object sender, EventArgs e)
        {
            btnGoToTable.Enabled = true;
        }

        private void RBfree_CheckedChanged(object sender, EventArgs e)
        {
            btnGoToTable.Enabled = false;
        }

        private void RBreserved_CheckedChanged(object sender, EventArgs e)
        {
            btnGoToTable.Enabled = false;
        }
        //go back to table overview
        private void btn_done_Click(object sender, EventArgs e)
        {
            //giving current payment number of current order
            currentPayment.OrderNumber = currentOrder.OrderNumber;
            //adding everything to the database
            paymentService.AddPayment(currentPayment.PaymentAmount, currentPayment.PaymentVat, currentPayment.PaymentType, currentPayment.Tip, currentPayment.CustomerComment, currentPayment.OrderNumber);

            pnlOrderingSystem.SelectedTab = tableViewTabCommentQ;
            string tableStatus = "free";
            tableService.ChangeTableStatus(currentTable.TableNumber, tableStatus);
            SetTablesColor();
            HideButtons();
            SetOrderDisplay();
            tabPageSettledBill.Hide();
            pnlTableStatus.Hide();
            pnlOrderingSystem.SelectedTab = tableViewTabCommentQ;
        }

        private void btn_payPartly_Click(object sender, EventArgs e)
        {
            lbl_tipAddedPartly.Hide();
            lbl_tipPartlyAdded.Hide();
            tabPagePayment.Hide();
            PayPartly.Show();
        }

        private void btn_SetCash_Click(object sender, EventArgs e)
        {
           
        }

        private void btn_SetCard_Click(object sender, EventArgs e)
        {
           
        }

        private void btn_payCashandCard_Click(object sender, EventArgs e)
        {
            if ((txtBox_cardAmount.Text == "") || (txtBox_Cash.Text == "")){

                MessageBox.Show("To continue the payment, you must fill in everything!");
                return;
            }
            PayPartly.Hide();
            tabPageAnyComments.Show();
        }

        private void btn_setTipPartly_Click(object sender, EventArgs e)
        {

            //display tip
            int customTipParlty = int.Parse(txtBox_tipPartly.Text);
            lbl_tipPartlyAdded.Text = "€ " + customTipParlty.ToString("0.00");

            //adding tip to database
            currentPayment.Tip = customTipParlty;

           
            label24.Hide();
            txtBox_tipPartly.Hide();
            btn_setTipPartly.Hide();
            lbl_tipAddedPartly.Show();
            lbl_tipPartlyAdded.Show();
        }
    }
}



