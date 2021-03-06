﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ThucTapNhom.Models;

namespace ThucTapNhom.Controllers.API
{
    public class ProductsController : ApiController
    {
        private MyDatabaseContext db = new MyDatabaseContext();

        // GET: api/Products
        public IHttpActionResult GetProducts(
            int page = 1, 
            int size = 10,
            int? status = null,
            string name = null,
            string startDate = null,
            string endDate = null,
            int? categoryId = null,
            int? minPrice = null,
            int? maxPrice = null
            )
        {
            var products = db.Products.AsQueryable();
            if (status != null)
            {
                products = products.Where(s => s.Status == status);
            }
            if (name != null)
            {
                products = products.Where(s => s.Name.Contains(name));
            }
            if (startDate != null)
            {
                var startDateFormat = Convert.ToDateTime(startDate);
                products = products.Where(s => s.CreatedAt >= startDateFormat);
            }
            if (endDate != null)
            {
                var tomorrow = Convert.ToDateTime(endDate).AddDays(1);
                products = products.Where(s => s.CreatedAt < tomorrow);
            }
            if (categoryId != null)
            {
                products = products.Where(s => s.CategoryId == categoryId);
            }
            if (minPrice != null)
            {
                products = products.Where(s => s.Price >= minPrice);
            }
            if (maxPrice != null)
            {
                products = products.Where(s => s.Price >= maxPrice);
            }
            var skip = (page - 1) * size;

            var total = products.Count();

            products = products
                .OrderBy(c => c.Id)
                .Skip(skip)
                .Take(size);

            return Ok(new PagedResult<Product>(products.ToList(), page, size, total));
        }

        // GET: api/Products/5
        [ResponseType(typeof(Product))]
        public IHttpActionResult GetProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/Products/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return BadRequest();
            }

            db.Entry(product).State = EntityState.Modified;

            try
            {
                product.UpdatedAt = DateTime.Now;
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Products
        [ResponseType(typeof(Product))]
        public IHttpActionResult PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            product.Status = 1;
            db.Products.Add(product);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [ResponseType(typeof(Product))]
        public IHttpActionResult DeleteProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            product.DeletedAt = DateTime.Now;
            product.Status = 0;
            db.SaveChanges();

            return Ok(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductExists(int id)
        {
            return db.Products.Count(e => e.Id == id) > 0;
        }
    }
}