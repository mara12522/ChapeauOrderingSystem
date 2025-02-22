﻿using System;
using System.Collections.Generic;
using System.Text;
using OrderingSystemModel;
using OrderingSystemDAL;

namespace OrderingSystemLogic
{
    public class OrderService
    {
        private OrderDAO orderDao;

        public OrderService()
        {
            orderDao = new OrderDAO();
        }
        public List<Order> GetAllOrders()
        { return orderDao.GetAllOrders(); }
        public void CreateNewOrder(Order order)
        {
            orderDao.CreateNewOrder(order);
        }
        public Order GetOrderNumber()
        {
            return orderDao.GetOrderNumber();
        }

        public List<Order> GetOrders()
        {
            return orderDao.GetAllOrders();
        }
        public Order GetOrder(int orderNumber)
        {
            return orderDao.GetOrder(orderNumber);
        }
    }
}
