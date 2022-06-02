﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using OrderingSystemModel;

namespace OrderingSystemDAL
{
    public class TableDAO : BaseDAO
    {
        public List<Table> GetAllTables()
        {
            string query = "SELECT TableNumber, TableStatus FROM [dbo].[TABLE]";
            SqlParameter[] sqlParameters = new SqlParameter[0];
            return ReadTables(ExecuteSelectQuery(query, sqlParameters));
        }

        public Table GetTable(int tableNumber)
        {
            string query = "SELECT TableNumber, TableStatus FROM TABLE WHERE TableNumber = @TableNumber";
            SqlParameter[] sqlParameters = new SqlParameter[1];
            sqlParameters[1] = new SqlParameter("@TableNumber", tableNumber);
            return ReadTable(ExecuteSelectQuery(query, sqlParameters));
        }

        private List<Table> ReadTables(DataTable dataTable)
        {

            List<Table> tables = new List<Table>();

            foreach (DataRow dr in dataTable.Rows)
            {
                Table table = new Table()
                {
                    TableNumber = (int)dr["TableNumber"],
                    TableStatus = (string)dr["TableStatus"],
                };
                tables.Add(table);
            }
            return tables;
        }

        private Table ReadTable(DataTable dataTable)
        {

            Table table = new Table();

            foreach (DataRow dr in dataTable.Rows)
            {
                table.TableNumber = (int)dr["TableNumber"];
                table.TableStatus = (string)dr["TableStatus"];
            }
            return table;
        }
    }
}
