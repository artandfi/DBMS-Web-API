using DBMS;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMS_Web_API.Controllers {
    [Route("api/Tables")]
    [ApiController]
    public class RowsController : ControllerBase {
        private readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        /// <summary>
        /// Gets the list of rows for the specified table
        /// </summary>
        /// <response code="200">_Gets the list of rows for the specified table_</response>
        /// <response code="400">_Database is not created yet_</response>
        /// <response code="404">_No table with such name in the database_</response>
        [HttpGet]
        [Route("{tableName}/Rows")]
        [ProducesResponseType(typeof(Response<List<Row>>), 200)]
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

            var response = new Response<List<Row>> {
                Value = table.Rows,
                Links = table.Rows.ToDictionary(r => $"Row {table.Rows.IndexOf(r)}", r => $"/Tables/{tableName}/Rows/{table.Rows.IndexOf(r)}")
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets a row with given index from the specified table
        /// </summary>
        /// <response code="200">_Gets a row with given index from the specified table_</response>
        /// <response code="400">_Database is not created yet_</response>
        /// <response code="404">_No table with such name in the database or no row at such index in the table_</response>
        [HttpGet]
        [Route("{tableName}/Rows/{id}")]
        [ProducesResponseType(typeof(Response<Row>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Get(string tableName, int id) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }

            try {
                var row = table.Rows[id];
                var response = new Response<Row> {
                    Value = row,
                    Links = new Dictionary<string, string> {
                        { "updateRow", $"/Tables/{tableName}/Rows/{id}" },
                        { "deleteRow", $"/Tables/{tableName}/Rows/{id}" }
                    }
                };

                return Ok(response);
            }
            catch {
                return NotFound(new { error = $"Theres is no row in the table named {tableName} at the specified index" });
            }
        }

        /// <summary>
        /// Adds a row to the specified table
        /// </summary>
        /// <response code="200">_Adds a row to the specified table_</response>
        /// <response code="400">_Database is not created yet, row values' and columns' number don't match, or row values are of invalid type_</response>
        /// <response code="404">_No table with such name in the database_</response>
        [HttpPost]
        [Route("{tableName}/Rows")]
        [ProducesResponseType(typeof(Response<Row>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Post(string tableName, [FromBody] Row row) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }
            
            if (row.Values.Count != table.Columns.Count) {
                return BadRequest(new { error = "Numbers of the row's values and the table's columns don't match" });
            }
            
            int tableId = _dbManager.Database.Tables.IndexOf(table);

            _dbManager.AddRow(tableId);
            for (int i = 0; i < row.Values.Count; i++) {
                if (!_dbManager.ChangeCellValue(row.Values[i], tableId, i, table.Rows.Count - 1)) {
                    _dbManager.DeleteRow(tableId, table.Rows.Count - 1);
                    return BadRequest(new { error = $"Value {row.Values[i]} is of invalid type" });
                }
            }

            var response = new Response<Row> {
                Value = row,
                Links = new Dictionary<string, string> {
                    { "updateRow", $"/Tables/{tableName}/Rows/{table.Rows.Count - 1}" },
                    { "deleteRow", $"/Tables/{tableName}/Rows/{table.Rows.Count - 1}" }
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Updates the row at specified index in the specified table
        /// </summary>
        /// <response code="200">_Updates the row at specified index in the specified table_</response>
        /// <response code="400">_Database is not created yet, row values' and columns' number don't match, or row values are of invalid type_</response>
        /// <response code="404">_No table with such name in the database or no row at such index in the table_</response>
        [HttpPut]
        [Route("{tableName}/Rows/{id}")]
        [ProducesResponseType(typeof(Response<Row>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Put(string tableName, int id, [FromBody] Row row) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }

            Row oldRow;
            try {
                oldRow = table.Rows[id];
            }
            catch {
                return NotFound(new { error = $"Theres is no row in the table named {tableName} at the specified index" });
            }

            if (row.Values.Count != table.Columns.Count) {
                return BadRequest(new { error = "Numbers of the row's values and the table's columns don't match" });
            }

            int tableId = _dbManager.Database.Tables.IndexOf(table);

            for (int i = 0; i < row.Values.Count; i++) {
                if (!_dbManager.ChangeCellValue(row.Values[i], tableId, i, id)) {
                    _dbManager.DeleteRow(tableId, table.Rows.Count - 1);
                    return BadRequest(new { error = $"Value {row.Values[i]} is of invalid type" });
                }
            }

            var response = new Response<Row> {
                Value = row,
                Links = new Dictionary<string, string> {
                    { "updateRow", $"/Tables/{tableName}/Rows/{id}" },
                    { "deleteRow", $"/Tables/{tableName}/Rows/{id}" }
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Deletes the row at specified index from the specified table
        /// </summary>
        /// <response code="200">_Deletes the row at specified index from the specified table_</response>
        /// <response code="400">_Database is not created yet_</response>
        /// <response code="404">_No table with such name in the database or no row at such index in the table_</response
        [HttpDelete]
        [Route("{tableName}/Rows/{id}")]
        [ProducesResponseType(typeof(Response<Row>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Delete(string tableName, int id) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }

            try {
                var rows = table.Rows;
                var row = rows[id];
                var response = new Response<Row> {
                    Value = row,
                    Links = new Dictionary<string, string> {
                        { "rows", $"/Tables/{tableName}/Rows" },
                        { "addRow", $"/Tables/{tableName}/Rows" }
                    }
                };

                rows.RemoveAt(id);
                return Ok(response);
            }
            catch {
                return NotFound(new { error = $"Theres is no row in the table named {tableName} at the specified index" });
            }
        }
    }
}
