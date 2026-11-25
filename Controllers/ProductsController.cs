using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;
using p48_lessons_asp.Models;

namespace p48_lessons_asp.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public ProductsController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _dataContext.Products.ToListAsync();
                if(products == null || products.Count == 0)
                {
                    return new ObjectResult(
                        new ResponseModel<object>
                        {
                            StatusCode = 404,
                            Message = "Products not found",
                        }
                    )
                    {
                        StatusCode = 404
                    };
                }
                return new ObjectResult(
                    new ResponseModel<List<Data.Entity.Product>>
                    {
                        StatusCode = 200,
                        Message = "Ok",
                        Data = products
                    }
                ){StatusCode = 200};
            }
            catch (OperationCanceledException ex)
            {
                // TODO: Logger
                Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.WriteLine($"[CRITICAL] Cannot create new product with reason: {ex.Message} | ${DateTime.Now} | ProductsController/CreateProduct");
                Console.ResetColor();
                

                return new ObjectResult(
                   new ResponseModel<object>
                   {
                       StatusCode = StatusCodes.Status500InternalServerError,
                       Message = "Cannot create product. See server logs",
                   })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductModel? model)
        {
            // TODO: Validation Logic
            Dictionary<string, string> validationResult = new();
            if (model == null)
            {
                return new ObjectResult(
                    new ResponseModel<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Product data is required"
                    })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            if (model.Name.Trim().Length == 0)
            {
                validationResult["nameError"] = "Product name is required";
            }

            if (model.Price <= 0)
            {
                validationResult["priceError"] = "Product price must be over zero";
            }
            if (validationResult.Count != 0)
            {
                return new ObjectResult(
                   new ResponseModel<object>
                   {
                       StatusCode = StatusCodes.Status400BadRequest,
                       Message = "Product data is required",
                       Data = validationResult
                   })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            try
            {
                // TODO: Repository Code
                var newProduct = new Data.Entity.Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description
                };
                _dataContext.Products.Add(newProduct);
                await _dataContext.SaveChangesAsync();
                return new ObjectResult(new ResponseModel<Data.Entity.Product>
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Product successfuly created",
                    Data = newProduct
                });
            }
            catch (OperationCanceledException ex)
            {
                // TODO: Logger
                Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.WriteLine($"[CRITICAL] Cannot create new product with reason: {ex.Message} | ${DateTime.Now} | ProductsController/CreateProduct");
                Console.ResetColor();
                

                return new ObjectResult(
                   new ResponseModel<object>
                   {
                       StatusCode = StatusCodes.Status500InternalServerError,
                       Message = "Cannot create product. See server logs",
                   })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }
        
        [HttpPost]
        [Route("{editType}Edit/{id}")]
        public async Task<IActionResult> EditProduct(string editType, int id, [FromBody] EditProductModel? model)
        {
            id--;
            if (editType != "full" && editType != "partial")
            {
                return new ObjectResult(
                    new ResponseModel<object>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Edit page not found"
                    })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            try {
                var product_count = await _dataContext.Products.CountAsync();
                if (id < 0 || id >= product_count)
                {
                    return new ObjectResult(
                        new ResponseModel<object>
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = $"Id must be between 1 and {product_count}"
                        })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                if (model is null)
                {
                    return new ObjectResult(
                        new ResponseModel<object>
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Product data is required"
                        })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                if (editType == "full")
                {
                    Dictionary<string, string> validationResult = new();
                    if (model.Name is null || model.Name.Trim().Length == 0)
                    {
                        validationResult["nameError"] = "Product name is required";
                    }
                    if (model.Price is null)
                    {
                        validationResult["priceError"] = "Product price must be included";
                    }
                    else if (model.Price <= 0)
                    {
                        validationResult["priceError"] = "Product price must be over zero";
                    }

                    if (validationResult.Count != 0)
                    {
                        return new ObjectResult(
                           new ResponseModel<object>
                           {
                               StatusCode = StatusCodes.Status400BadRequest,
                               Message = "Product data is required",
                               Data = validationResult
                           })
                        {
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                    }
                }
                var product_list = await _dataContext.Products.Where(p => p.DeletedAt == null).ToListAsync();
                var product = product_list[id];
                product.Name =  model.Name is null ? product.Name : model.Name;
                product.Description = model.Description == "REMOVE" ? null : (model.Description is null ? product.Description : model.  Description);
                product.Price = model.Price is null ? product.Price : model.Price.Value;
                _dataContext.Products.Update(product);
                await _dataContext.SaveChangesAsync();
                return new ObjectResult(new ResponseModel<Data.Entity.Product>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Product successfuly updated",
                    Data = product
                });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.WriteLine($"[CRITICAL] Cannot interact with database with reason: {ex.Message} | ${DateTime.Now} | ProductsController/EditProduct");
                Console.ResetColor();
                

                return new ObjectResult(
                   new ResponseModel<object>
                   {
                       StatusCode = StatusCodes.Status500InternalServerError,
                       Message = "Cannot interact with database. See server logs",
                   })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        [HttpDelete]
        [Route("{deleteType}Delete/{id}")]
        public async Task<IActionResult> DeleteProduct(string deleteType, int id)
        {
            id--;
            if (deleteType.ToLower() != "soft" && deleteType.ToLower() != "hard")
            {
                return new ObjectResult(
                    new ResponseModel<object>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Delete page not found"
                    })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            try {
                var product_count = await _dataContext.Products.CountAsync();
                if (id < 0 || id >= product_count)
                {
                    return new ObjectResult(
                        new ResponseModel<object>
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = $"Id must be between 1 and {product_count}"
                        })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var product_list = await _dataContext.Products.Where(p => p.DeletedAt == null).ToListAsync();
                var product = product_list[id];
                if (deleteType.ToLower() == "soft")
                {
                    product.DeletedAt = DateTime.Now;
                    _dataContext.Update(product);
                }
                else
                {
                    _dataContext.Remove(product);
                }
                await _dataContext.SaveChangesAsync();
                return new ObjectResult(new ResponseModel<Data.Entity.Product>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Product successfuly deleted"
                });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                System.Console.WriteLine($"[CRITICAL] Cannot interact with database with reason: {ex.Message} | ${DateTime.Now} | ProductsController/EditProduct");
                Console.ResetColor();
                

                return new ObjectResult(
                   new ResponseModel<object>
                   {
                       StatusCode = StatusCodes.Status500InternalServerError,
                       Message = "Cannot interact with database. See server logs",
                   })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        #region Old_example_without_db 
        /*
        private readonly IGenerator<Product> _generator;
        private readonly List<Product> products;
        public ProductsController(IGenerator<Product> generator)
        {
            _generator = generator;
            products = _generator.Generate(100);

        }

        [HttpGet]
        public IActionResult Products([FromQuery] GetProductQueryModel? model)
        {
            System.Console.WriteLine(model);
            return Ok(new
            {
                Status = 200,
                Message = "Ok",
                Products = products
            });
        }
        [HttpGet]
        [Route("query")]
        public IActionResult GetProductsIn([FromQuery] int? startFrom, [FromQuery] int? endsWith)
        {
            if (startFrom.HasValue)
            {
                if (startFrom.Value < 0 || startFrom.Value > products.Count - 1)
                {
                    return BadRequest(new
                    {
                        Status = 400,
                        Message = $"Start from must be in 0-{products.Count}"
                    });
                }
                return Ok(products.Slice(startFrom.Value, (products.Count - 1) - startFrom.Value));
            }
            if (endsWith.HasValue)
            {
                if (endsWith < products.Count || endsWith > products.Count - 1)
                {
                    return BadRequest(new
                    {
                        Status = 400,
                        Message = $"endsWith must be in 0-{products.Count - 1}"
                    });
                }
            }
            return NotFound();
        }

        //Реалізація рут параметру
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            if (id > (products.Count - 1)) return NotFound(new
            {
                Status = 404,
                Message = $"Product with id-{id} not found",
                Data = (object)null!
            });
            // return Ok(products[id]);
            return Ok(new
            {
                Status = 200,
                Message = "Ok",
                Data = products[id]
            });
        }
        // POST: api/products/

        // [FromBody] Only Json body
        // [FromForm] Використовується для форм з html, + надає можливість отримувати файли від користувача
        
        // public IActionResult CreateProducts([FromBody] CreateProductModel? model)
        [HttpPost]
        public async Task<IActionResult> CreateProducts([FromForm] CreateProductModel? model)
        {
            if (model is null)
            {
                // return BadRequest();
                return new ObjectResult(new { message = "Product body is required" }) { StatusCode = StatusCodes.Status400BadRequest };
            }
            if (model.productImage != null)
            {
                var file = model.productImage;
                System.Console.WriteLine("File name: {0}", file.FileName);
                // System.Console.WriteLine("Content: {0}", file.ToString());
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                using(var stream = file.OpenReadStream())
                using(var reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    System.Console.WriteLine(content);
                }
                Console.ResetColor();
            }
            System.Console.WriteLine(model.Product);
            return new ObjectResult(new { status = 201, message = "Created", data = model.Product }) { StatusCode = StatusCodes.Status201Created };
        }

        */
        #endregion
    }

}
