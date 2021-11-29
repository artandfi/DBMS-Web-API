using Microsoft.AspNetCore.Mvc;
using DBMS;
using System.Collections.Generic;

namespace DBMS_Web_API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class DatabaseController : ControllerBase {
        private readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        /// <summary>
        /// Gets the current database
        /// </summary>
        /// <response code="200">_Gets the current database_</response>
        /// <response code="400">_Database is not created yet_</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<Database>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public IActionResult Get() {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var response = new Response<Database> {
                Value = _dbManager.Database,
                Links = new Dictionary<string, string> {
                    { "updateDatabase", "/Database/{newName}" },
                    { "deleteDatabase", "/Database" },
                    { "tables", "/Tables" },
                    { "createTable", "/Tables/{name}" },
                }
            };

            if (_dbManager.Database.Tables.Count > 0) {
                response.Links.Add("updateTable", "/Tables/{oldName}/{newName}");
                response.Links.Add("deleteTable", "/Tables/{name}");
            }

            return Ok(_dbManager.Database);
        }

        /// <summary>
        /// Creates a new database with the specified name
        /// </summary>
        /// <response code="200">_Creates a new database with the specified name_</response>
        /// <response code="400">_Database is already created_</response>
        [HttpPost("{name}")]
        [ProducesResponseType(typeof(Response<Database>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public IActionResult Post(string name) {
            if (_dbManager.Database != null) {
                return BadRequest(new { error = "Database is already created" });
            }

            _dbManager.CreateDatabase(name);

            var response = new Response<Database> {
                Value = _dbManager.Database,
                Links = new Dictionary<string, string> {
                    { "updateDatabase", "/Database/{newName}" },
                    { "deleteDatabase", "/Database" },
                    { "tables", "/Tables" },
                    { "createTable", "/Tables/{name}" }
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Updates the name of the current database
        /// </summary>
        /// <response code="200">_Creates a new database with the specified name_</response>
        /// <response code="400">_Database is not created yet_</response>
        [HttpPut("{name}")]
        [ProducesResponseType(typeof(Response<Database>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public IActionResult Put(string name) {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            _dbManager.Database.Name = name;

            var response = new Response<Database> {
                Value = _dbManager.Database,
                Links = new Dictionary<string, string> {
                    { "updateDatabase", "/Database/{newName}" },
                    { "deleteDatabase", "/Database" },
                    { "tables", "/Tables" },
                    { "createTable", "/Tables/{name}" }
                }
            };

            if (_dbManager.Database.Tables.Count > 0) {
                response.Links.Add("updateTable", "/Tables/{oldName}/{newName}");
                response.Links.Add("deleteTable", "/Tables/{name}");
            }

            return Ok(response);
        }

        /// <summary>
        /// Deletes the current database
        /// </summary>
        /// <response code="200">_Deletes the current database_</response>
        /// <response code="400">_Database is not created yet_</response>
        [HttpDelete]
        [ProducesResponseType(typeof(Response<Database>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public IActionResult Delete() {
            if (_dbManager.Database == null) {
                return BadRequest(new { error = "Database is not created yet" });
            }

            var database = _dbManager.Database;
            var response = new Response<Database> {
                Value = database,
                Links = new Dictionary<string, string> {
                    { "createDatabase", "/Database/{name}" }
                }
            };

            _dbManager.Database = null;
            return Ok(response);
        }
    }
}
