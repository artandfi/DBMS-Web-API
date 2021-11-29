using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBMS;

namespace DBMS_Web_API.Controllers {
    [Route("api/Tables")]
    [ApiController]
    [Produces("application/json")]
    public class ColumnsController : ControllerBase {
        private readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        /// <summary>
        /// Gets the list of columns for the specified table
        /// </summary>
        /// <response code="200">_Gets the list of columns for the specified table_</response>
        /// <response code="400">_Database is not created yet_</response>
        /// <response code="404">_No table with such name in the database_</response>
        [HttpGet]
        [Route("{tableName}/Columns")]
        [ProducesResponseType(typeof(Response<List<Column>>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Get(string tableName) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }

            var response = new Response<List<Column>> {
                Value = table.Columns,
                Links = table.Columns.ToDictionary(c => $"{c.Name} ({c.Type})", c => $"/Tables/{tableName}/Columns/{c.Name}")
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets a column with the specified name from the specified table
        /// </summary>
        /// <response code="200">_Gets a column with the specified name from the specified table_</response>
        /// <response code="400">_Database is not created yet_</response>
        /// <response code="404">_No table or no column with such name in the database_</response>
        [HttpGet]
        [Route("{tableName}/Columns/{columnName}")]
        [ProducesResponseType(typeof(Response<Column>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Get(string tableName, string columnName) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }

            var column = table.Columns.Find(c => c.Name.Equals(columnName));
            if (column == null) {
                return NotFound(new { error = $"There is no column named {columnName} in the table {tableName}" });
            }

            var response = new Response<Column> {
                Value = column,
                Links = new Dictionary<string, string> {
                    { "updateColumn", $"/Tables/{tableName}/Columns/{column.Name}/{{newColumnName}}" },
                    { "deleteColumn", $"/Tables/{tableName}/Columns/{column.Name}" }
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Creates a column with the specified name and adds it to the specified table
        /// </summary>
        /// <response code="200">_Creates a column with the specified name and adds it to the specified table_</response>
        /// <response code="400">_Database is not created yet_</response>
        /// <response code="404">_No table with such name in the database_</response>
        [HttpPost]
        [Route("{tableName}/Columns/{columnName}/{columnType}")]
        [ProducesResponseType(typeof(Response<Column>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Post(string tableName, string columnName, string columnType) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }
            int id = _dbManager.Database.Tables.IndexOf(table);

            var column = DatabaseManager.ColumnFromString(columnName, columnType);
            if (!_dbManager.AddColumn(id, column)) {
                return BadRequest(new { error = $"Table {tableName} already contains the column named {columnName}" });
            }

            var response = new Response<Column> {
                Value = column,
                Links = new Dictionary<string, string> {
                    { "updateColumn", $"/Tables/{tableName}/Columns/{column.Name}/{{newColumnName}}" },
                    { "deleteColumn", $"/Tables/{tableName}/Columns/{column.Name}" }
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Updates the name of the specified column in the specified table
        /// </summary>
        /// <response code="200">_Updates the name of the specified column in the specified table_</response>
        /// <response code="400">_Database is not created yet_</response>
        /// <response code="404">_No table or no column with such name in the database_</response>
        [HttpPut]
        [Route("{tableName}/Columns/{oldColumnName}/{newColumnName}")]
        [ProducesResponseType(typeof(Response<Column>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Put(string tableName, int oldColumnName, string newColumnName) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }
            int id = _dbManager.Database.Tables.IndexOf(table);

            var column = table.Columns.Find(c => c.Name.Equals(oldColumnName));
            if (column == null) {
                return NotFound(new { error = $"There is no column named {oldColumnName} in the table {tableName}" });
            }

            if (_dbManager.GetColumnNames(id).Contains(newColumnName) && !newColumnName.Equals(oldColumnName)) {
                return BadRequest(new { error = $"Table {tableName} already contains the column named {newColumnName}" });
            }

            var response = new Response<Column> {
                Value = column,
                Links = new Dictionary<string, string> {
                    { "updateColumn", $"/Tables/{tableName}/Columns/{column.Name}/{{newColumnName}}" },
                    { "deleteColumn", $"/Tables/{tableName}/Columns/{column.Name}" }
                }
            };

            column.Name = newColumnName;
            return Ok(response);
        }

        /// <summary>
        /// Deletes the specified column from the specified table
        /// </summary>
        /// <response code="200">_Deletes the specified column from the specified table_</response>
        /// <response code="400">_Database is not created yet_</response>
        /// <response code="404">_No table or no column with such name in the database_</response>
        [HttpDelete]
        [Route("{tableName}/Columns/{columnName}")]
        [ProducesResponseType(typeof(Response<Column>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Delete(string tableName, int columnName) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }
            int tableId = _dbManager.Database.Tables.IndexOf(table);

            var column = table.Columns.Find(c => c.Name.Equals(columnName));
            if (column == null) {
                return NotFound(new { error = $"There is no column named {columnName} in the table {tableName}" });
            }
            int columnId = table.Columns.IndexOf(column);

            var response = new Response<Column> {
                Value = column,
                Links = new Dictionary<string, string> {
                    { "addColumn", $"/Tables/{tableName}/Columns/{{columnName}}/{{columnType}}" }
                }
            };

            _dbManager.DeleteColumn(tableId, columnId);
            return Ok(response);
        }
    }
}
