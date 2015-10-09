/*
 * Copyright 2006-2015 TIBIC SOLUTIONS
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace LWAS.CustomControls.DataControls
{
	public class SqlDataBridge
	{
        public class ExecuteSqlEventArgs : EventArgs
        {
            public SqlCommand Command;
        }
        public event EventHandler<ExecuteSqlEventArgs> ExecuteSql;

		private SqlConnection _connection;
		private string _connectionString;
		private Dictionary<string, SqlCommand> _commands = new Dictionary<string, SqlCommand>();
		public SqlConnection Connection
		{
			get { return this._connection; }
			set { this._connection = value; }
		}
		public string ConnectionString
		{
			get { return this._connectionString; }
			set
			{
				this._connectionString = value;
				this.Open();
			}
		}
		public Dictionary<string, SqlCommand> Commands
		{
			get { return this._commands; }
			set { this._commands = value; }
		}

		public void Open()
		{
			if (string.IsNullOrEmpty(this._connectionString))
			{
				throw new InvalidOperationException("ConnectionString not set");
			}
			this.Close();
			this._connection = new SqlConnection(this._connectionString);
			this._connection.Open();
		}
		public void Close()
		{
			if (this._connection != null && this._connection.State == ConnectionState.Open)
			{
				this._connection.Close();
			}
		}
		public SqlDataReader ExecuteCommand(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			if (!this._commands.ContainsKey(key))
			{
				throw new InvalidOperationException(string.Format("Unknown command '{0}'", key));
			}
			if (this._connection == null || this._connection.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Invalid connection");
			}

            var cmd = _commands[key];
			cmd.Connection = _connection;

            if (this.ExecuteSql != null)
                ExecuteSql(this, new ExecuteSqlEventArgs() { Command = cmd });

			return cmd.ExecuteReader();
		}
		public SqlCommand DiscoverCommand(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (this._connection == null || this._connection.State != ConnectionState.Open)
			{
				throw new InvalidOperationException("Invalid connection");
			}
			SqlCommand command = new SqlCommand(name, this._connection);
			command.CommandType = CommandType.StoredProcedure;
			SqlCommandBuilder.DeriveParameters(command);
			return command;
		}
	}
}
