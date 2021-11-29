using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBMS;

namespace DBMS_Web_API.Controllers {
    [Route("api/Tables/Project")]
    [ApiController]
    public class ProjectionController : ControllerBase {
        private readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        /// <summary>
        /// Gets the projection of the specified table by specified columns
        /// </summary>
        /// <response code="200">_Gets the projection of the specified table by specified columns_</response>
        /// <response code="400">_Database is not created yet or number of columns provided is greater than number of columns in the specified table_</response>
        /// <response code="404">_There is no table or no columns with such name_</response>
        [HttpGet("{tableName}/{columnNames}")]
        [ProducesResponseType(typeof(Response<Table>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult Get(string tableName, string columnNames) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var table = _dbManager.Database.Tables.Find(t => t.Name.Equals(tableName));
            if (table == null) {
                return NotFound(new { error = $"There is no table named {tableName} in the database" });
            }
            int tableIndex = _dbManager.Database.Tables.IndexOf(table);

            var columnNamesList = columnNames.Replace(" ", "").Split(",").ToList();
            if (columnNamesList.Count > table.Columns.Count) {
                return BadRequest(new { error = $"Number of columns provided is greater than number of columns in the table named {tableName}" });
            }

            var indices = new List<int>();
            foreach (string columnName in columnNamesList) {
                var column = table.Columns.Find(c => c.Name.Equals(columnName));
                if (column == null) {
                    return NotFound(new { error = $"There is no column named {columnName} in the table named {tableName}" });
                }

                indices.Add(table.Columns.IndexOf(column));
            }

            var columns = indices.Select(i => table.Columns[i]);
            var response = new Response<Table> {
                Value = _dbManager.Project(tableIndex, indices.ToArray()),
                Links = columns.ToDictionary(c => $"{c.Name} ({c.Type})", c => $"/Tables/{tableName}/Columns/{c.Name}")
            };
            response.Links.Add("table", $"/Tables/{tableName}");

            return Ok(response);
        }
    }
}
