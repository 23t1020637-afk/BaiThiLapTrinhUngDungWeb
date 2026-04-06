using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.Admin.AppCodes;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Catalog;
using SV23T1020637.Models.Common;
using System;
using System.Buffers;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV23T1020637.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến mặt hàng
    /// </summary>
    [Authorize(Roles = $"{WebUserRoles.DataManager},{WebUserRoles.Administrator}")]
    public class ProductController : Controller
    {
        readonly int PageSize = 10;
        readonly string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";

        // GET: Products
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Quản lý Mặt hàng";
            ProductSearchInput? condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION);
            condition ??= new ProductSearchInput()
            {
                CategoryID = 0,
                SupplierID = 0,
                MaxPrice = 0,
                MinPrice = 0,
                SearchValue = "",
                Page = 1,
                PageSize = PageSize
            };
            return View(condition);
        }
        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            input.PageSize = PageSize;
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, input);
            return View(result);
        }
        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var products = await CatalogDataService.GetProductAsync(id);
            if (products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        // GET: Product/Create
        public async Task<IActionResult> Create()
        {
            return View("Edit", new Product());
        }



        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Product = await CatalogDataService.GetProductAsync(id);
            if (Product == null)
            {
                return NotFound();
            }
            return View(Product);
        }


        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Product = await CatalogDataService.GetProductAsync(id);
            if (Product == null)
            {
                return NotFound();
            }
            ViewBag.isUsed = await CatalogDataService.IsUsedProductAsync(id);
            return View(Product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Product = await CatalogDataService.GetProductAsync(id);
            if (Product != null)
            {
                await CatalogDataService.DeleteProductAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveData(Product product, IFormFile? uploadPhoto)
        {
            try
            {
                #region TODO: KIểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(product.ProductName))
                    ModelState.AddModelError(nameof(product.ProductName), "Tên sản phẩm không được để trống.!");
                if (product.CategoryID == 0)
                    ModelState.AddModelError(nameof(product.CategoryID), "Hãy chọn loại hàng!");
                if (product.SupplierID == 0)
                    ModelState.AddModelError(nameof(product.SupplierID), "Hãy chọn nhà cung cấp !");
                if (string.IsNullOrWhiteSpace(product.Unit))
                    ModelState.AddModelError(nameof(product.Unit), "Tên đơn vị không được để trống.!");
                if (product.Price == 0)
                    ModelState.AddModelError(nameof(product.Price), "giá cả không được để trống!");
                if (uploadPhoto == null && string.IsNullOrEmpty(product.Photo))
                    ModelState.AddModelError(nameof(product.Photo), "Vui lòng upload ảnh");
                // kiểm tra email phone
                if (!ModelState.IsValid)
                    return View("Edit", product);
                #endregion
                #region Tiền xử lý dữ liệu trước khi lưu vào database

                #endregion
                #region lưu vào CSDL
                // xử lý ảnh
                if (uploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}-{uploadPhoto.FileName}";
                    string filePath = Path.Combine(ApplicationContext.WWWRootPath, @"images\products", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadPhoto.CopyTo(stream);
                    }
                    product.Photo = fileName;
                }
                if (product.ProductID != 0)
                {
                    await CatalogDataService.UpdateProductAsync(product);
                    return View(nameof(Edit), product);
                }
                await CatalogDataService.AddProductAsync(product);
                #endregion
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                //TODO: Ghi log lỗi căn cứ vào ex.Message và ex.StackTrace
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", product);
            }
        }
        #region Photo
        public async Task<IActionResult> CreatePhoto(int _ProductID)
        {
            return View("EditPhoto", new ProductPhoto()
            {
                ProductID = _ProductID
            });
        }
        public async Task<IActionResult> EditPhoto(int ProductID, int photoId)
        {
            var photos = await CatalogDataService.ListPhotosAsync(ProductID);

            // Tìm ảnh theo photoId trong danh sách
            var photo = photos.FirstOrDefault(p => p.PhotoID == photoId);
            return View(photo);
        }
        public async Task<IActionResult> DeletePhoto(int ProductID, int photoId)
        {
            await CatalogDataService.DeletePhotoAsync(photoId);
            return RedirectToAction("Edit", new { id = ProductID });
        }
        [HttpPost]
        public async Task<IActionResult> SavePhotoAsync(ProductPhoto data, IFormFile? uploadPhoto)
        {
            ViewBag.Title = data.PhotoID == 0 ? "Bổ sung ảnh cho mặt hàng" : "Thay đổi ảnh của mặt hàng";

            if (uploadPhoto == null && string.IsNullOrEmpty(data.Photo))
                ModelState.AddModelError(nameof(data.Photo), "Vui lòng upload ảnh");

            if (string.IsNullOrWhiteSpace(data.Description))
                ModelState.AddModelError(nameof(data.Description), "Mô tả hình ảnh không được để trống");

            if (data.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị không hợp lệ");



            if (!ModelState.IsValid)
            {
                return View("EditPhoto", data);
            }

            if (uploadPhoto != null)
            {
                string fileName = $"{DateTime.Now.Ticks}-{uploadPhoto.FileName}";
                string filePath = Path.Combine(ApplicationContext.WWWRootPath, @"images\products", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }
                data.Photo = fileName;
            }

            if (data.PhotoID == 0)
                await CatalogDataService.AddPhotoAsync(data);
            else
                await CatalogDataService.UpdatePhotoAsync(data);

            return RedirectToAction("Edit", new { id = data.ProductID });
        }
        #endregion


        public async Task<IActionResult> CreateAttribute(int _ProductID)
        {
            return View("EditAttribute", new ProductAttribute()
            {
                ProductID = _ProductID
            });
        }
        public async Task<IActionResult> EditAttribute(int attributeId, int ProductID)
        {
            var attributes = await CatalogDataService.ListAttributesAsync(ProductID);

            // Tìm ảnh theo photoId trong danh sách
            var attribute = attributes.FirstOrDefault(p => p.AttributeID == attributeId);
            return View(attribute);
        }
        public async Task<IActionResult> DeleteAttributeAsyn(int ProductID, int photoId)
        {
            await CatalogDataService.DeleteAttributeAsync(photoId);
            return RedirectToAction("Edit", new { id = ProductID });
        }
        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            if (string.IsNullOrWhiteSpace(data.AttributeName))
                ModelState.AddModelError(nameof(data.AttributeName), "Tên thuộc tính không được để trống");
            if (string.IsNullOrWhiteSpace(data.AttributeValue))
                ModelState.AddModelError(nameof(data.AttributeValue), "Giá trị thuộc tính không được để trống");

            if (data.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị không hợp lệ");

            //if (data.AttributeID > 0)
            //{
            //    ProductAttribute? currentAttribute = await CatalogDataService.GetAttributeAsync(data.AttributeID);

            //    if (currentAttribute != null && data.DisplayOrder != currentAttribute.DisplayOrder)
            //    {
            //        if (await CatalogDataService.InUsedDisplayOrderOfAttribute(data.ProductID, data.DisplayOrder))
            //        {
            //            ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị đã tồn tại. Vui lòng nhập lại");
            //        }
            //    }
            //}
            //else
            //{
            //    if (await CatalogDataService.InUsedDisplayOrderOfAttribute(data.ProductID, data.DisplayOrder))
            //    {
            //        ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị đã tồn tại. Vui lòng nhập lại");
            //    }
            //}


            if (!ModelState.IsValid)
            {
                return View("EditAttribute", data);
            }

            if (data.AttributeID == 0)
            {
                await CatalogDataService.AddAttributeAsync(data);
            }
            else
            {
                await CatalogDataService.UpdateAttributeAsync(data);
            }

            return RedirectToAction("Edit", new { id = data.ProductID });
        }
        public async Task<IActionResult> DeleteAttribute(int ProductID, int AttributeID)
        {
            await CatalogDataService.DeleteAttributeAsync(AttributeID);
            return RedirectToAction("Edit", new { id = ProductID });
        }
    }
}
    


