using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using HomeGardenShopDAL.Entities;
using HomeGardenShopDAL.UnitOfWork;
using HomeGardenShopServer;
using HomeGardenShopServer.Interface;
using HomeGardenShopServer.Models;
using HomeGardenShopServer.Moduls;
using Ninject;
using static System.Net.Mime.MediaTypeNames;

namespace HomeGardenShopServer.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private IKernel kernel;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
        try
        {
            kernel = new StandardKernel(new ProductNinjectModule());
        }
        catch (Exception ex)

        {

        }
    }

   

    //public override Task<ProductsReply> GetProducts(ProductsRequest request, ServerCallContext context)
    //{
    //    return Task.FromResult(new ProductsReply
    //    {
    //        Cost = "1",
    //         Name = "Tomato"
            
    //    }); ;
    //}
    public override async Task ListProducts(ProductsRequest request, IServerStreamWriter<ProductsReply> responseStream, ServerCallContext context)
    {
        Console.WriteLine("ListProducts");
        List<Product> products = new List<Product>();
        try
        {
            IService<Product> productService = kernel.Get<IService<Product>>();
            var list = await productService.GetAll();
            foreach (var product in list.ToList())
            {
                products.Add(product);


            }

            foreach (var item in products)
            {
                ProductsReply productsReply = new ProductsReply();
                if (request.Language == "ru-RU")
                {
                    productsReply.Name = item.Name;
                    productsReply.Description = item.Description;
                }
                else if (request.Language == "uk-UA")
                {
                    productsReply.Name = item.NameUA;
                    productsReply.Description = item.DescriptionUA;
                }
                //en-US
                else
                {
                    productsReply.Name = item.NameEN;
                    productsReply.Description = item.DescriptionEN;
                }
                productsReply.Price = item.Price;
                productsReply.Count = item.Count;
                productsReply.Id = item.Id;
                productsReply.CategoryId = item.CategoryId;
                productsReply.DiscountPrice = item.DiscountPrice;
                productsReply.Image = ByteString.CopyFrom(item.Image);
                await responseStream.WriteAsync(productsReply);
            }
        }
        catch (Exception ex)
        {

        }
       
    }
    public override async Task ListCategorys(CategorysRequest request, IServerStreamWriter<CategorysReply> responseStream, ServerCallContext context)
    {
        try
        {
            IService<Category> categoryService = kernel.Get<IService<Category>>();
            List<Category> categorys = new List<Category>();
            var list = await categoryService.GetAll();
            foreach (var category in list.ToList())
            {
                categorys.Add(category);

            }

            foreach (var item in categorys)
            {
                CategorysReply categorysReply = new CategorysReply();
                if (request.Language == "ru-RU")
                    categorysReply.Name = item.Name;
                else if (request.Language == "uk-UA")
                    categorysReply.Name = item.NameUA;
                //en-US
                else
                    categorysReply.Name = item.NameEN;

                categorysReply.Id = item.Id;
                await responseStream.WriteAsync(categorysReply);
            }
        }
        catch (Exception ex)
        {

        }
       
    }

    public override async Task MakeAnOrder(MakeAnOrderRequest request, IServerStreamWriter<MakeAnOrderReply> responseStream, ServerCallContext context)
    {

        MakeAnOrderReply makeAnOrderReply = new MakeAnOrderReply();
        try
        {
            IService<Order> orderService = kernel.Get<IService<Order>>();
            if (request != null)
            {
                Order order = new Order();
                order.Id = await orderService.GetLast() + 1;
                order.UserId = request.UserId;
                order.StatusId = (int)OrderStatus.InProcess;
                order.Sum = request.Sum;
                order.DateTime = DateTime.Now;
                await orderService.AddOrUpdate(order);
                await orderService.Save();
                makeAnOrderReply.OrderId = order.Id;
                IService<ProductOrder> producOrderService = kernel.Get<IService<ProductOrder>>();
                if (request.Products != null)
                {
                    foreach (var product in request.Products)
                    {
                        ProductOrder productOrder = new ProductOrder();
                        productOrder.ProductId = product.Id;
                        productOrder.Price = product.Price;
                        productOrder.Count = product.Count;
                        productOrder.OrderId = order.Id;
                        await producOrderService.AddOrUpdate(productOrder);
                    }
                    makeAnOrderReply.StatusId = order.StatusId;
                }
                else
                {
                    makeAnOrderReply.StatusId = (int)OrderStatus.Error;
                }
                await producOrderService.Save();
            }
            else
            {
                makeAnOrderReply.StatusId = (int)OrderStatus.Error;
            }

            await responseStream.WriteAsync(makeAnOrderReply);
        }
        catch (Exception ex)
        {

        }
    }

    public override async Task<ListOrdersReply> ListOrders(ListOrdersRequest request, ServerCallContext context)
    {
        IKernel kernel = new StandardKernel(new ProductNinjectModule());
        List<Product> products = new List<Product>();
        RepeatedField<OrderGrpc> orders = new RepeatedField<OrderGrpc>();
        try
        {
            IService<Order> orderService = kernel.Get<IService<Order>>();
            IService<ProductOrder> producOrderService = kernel.Get<IService<ProductOrder>>();
            var list = await orderService.GetAll();
            var productList = await producOrderService.GetAll();


           // int userId = int.Parse(request.UserId);
            var userOrders = list.ToList().Where(x => x.UserId == request.UserId).ToList();

            if (userOrders.Count > 0)
            {
                foreach (var order in userOrders)
                {
                    var userProdList = productList.Where(x => x.OrderId == order.Id).ToList();
                    RepeatedField<ProductsReply> productsRep = new RepeatedField<ProductsReply>();

                    foreach (var item in userProdList)
                    {
                        ProductsReply productsReply = new ProductsReply();
                        productsReply.Price = item.Price;
                        productsReply.Count = item.Count;
                        productsReply.Id = item.ProductId;
                        productsRep.Add(productsReply);
                    }

                    OrderGrpc orderGrpc = new OrderGrpc
                    {
                        UserId = order.UserId,
                        OrderId = order.Id,
                        StatusId = order.StatusId,
                        Sum = order.Sum,
                        DateTime = order.DateTime.ToString(),
                        Products = { productsRep }
                    };
                    orders.Add(orderGrpc);
                    //await responseStream.WriteAsync(listOrdersReply);
                }
            }
        }
        catch (Exception ex)
        {
        }
        return await Task.FromResult(new ListOrdersReply
        {
             Orders = { orders }

        });



    }

    public async override Task<CancelOrderReply> CancelOrder(CancelOrderRequest request, ServerCallContext context)
    {
        bool res = false;
        try
        {
            IService<Order> orderService = kernel.Get<IService<Order>>();

            Order order = await orderService.Get(request.OrderId);
           if(order.UserId == request.UserId)
            {
                order.StatusId = (int)OrderStatus.Canceled;
                await orderService.AddOrUpdate(order);
                await orderService.Save();
                res = true;
            }
           else
            {
                res = false;
            }
            res = true;
        }
        catch(Exception ex)
        {
            res = false;
        }
       

        return await Task.FromResult(new CancelOrderReply
        {
          IsCancel = res

        });
    }
    public override async Task ListNews(NewsRequest request, IServerStreamWriter<NewsReply> responseStream, ServerCallContext context)
    {
        try
        {
            IService<News> newsService = kernel.Get<IService<News>>();
            List<News> news = new List<News>();
            var list = await newsService.GetAll();
            foreach (var item in list.ToList())
            {
                news.Add(item);

            }

            foreach (var item in news)
            {
                NewsReply newsReply = new NewsReply();
                if (request.Language == "ru-RU")
                {
                    newsReply.Name = item.Name;
                    newsReply.Description = item.Description;
                }
                else if (request.Language == "uk-UA")
                {
                    newsReply.Name = item.NameUA;
                    newsReply.Description = item.DescriptionUA;
                }
                //en-US
                else
                {
                    newsReply.Name = item.NameEN;
                    newsReply.Description = item.DescriptionEN;
                }
                newsReply.Id = item.Id;
                newsReply.Image = ByteString.CopyFrom(item.Image);
                newsReply.DateTime = item.DateTime.ToString();
                await responseStream.WriteAsync(newsReply);
            }
        }
        catch (Exception ex)
        {

        }

    }

    public async override Task<RegistrUserReply> RegistrUser(RegistrUserRequest request, ServerCallContext context)
    {
        bool res = false;
        try
        {
            IService<User> userService = kernel.Get<IService<User>>();
            var userList = await userService.GetAll();
            var userDB = userList.Where(x => x.UserId == request.UserId).FirstOrDefault();

            if(userDB != null)
            {
                userDB.Name = request.Name;
                userDB.Address = request.Address;
                userDB.Mail = request.Mail;
                userDB.Phone = request.Phone;
                await userService.AddOrUpdate(userDB);
                await userService.Save();
            }
            else
            {
                User user = new User();
                user.UserId = request.UserId;
                user.Name = request.Name;
                user.Address = request.Address;
                user.Mail = request.Mail;
                user.Phone = request.Phone;
                await userService.AddOrUpdate(user);
                await userService.Save();
            }

            res = true;
        }
        catch (Exception ex)
        {
            res = false;
        }

        return await Task.FromResult(new RegistrUserReply
        {
            IsRegistr = res

        });
    }
    public async override Task<IsRegistrUserReply> IsRegistrUser(IsRegistrUserRequest request, ServerCallContext context)
    {
        bool res = false;
        bool isRegistr = false;
        try
        {
            IService<User> userService = kernel.Get<IService<User>>();
            var userList = await userService.GetAll();
            var userDB = userList.Where(x => x.UserId == request.UserId).FirstOrDefault();

            if (userDB != null)
            {
                isRegistr = true;
            }
            else
            {
                isRegistr = false;
            }

            res = true;
        }
        catch (Exception ex)
        {
            res = false;
        }

        return await Task.FromResult(new IsRegistrUserReply
        {
            IsRegistr = isRegistr,
            IsError = res
        }); 
    }

    public async override Task<GetUserReply> GetUser(GetUserRequest request, ServerCallContext context)
    {
        bool res = false;
        User user = new User();
        try
        {
            IService<User> userService = kernel.Get<IService<User>>();
            var userList = await userService.GetAll();
            var _user = userList.Where(x => x.UserId == request.UserId).FirstOrDefault();
            if (_user != null)
            {
                user = _user;
                res = true;
            }
        }
        catch (Exception ex)
        {
            res = false;
        }

        return await Task.FromResult(new GetUserReply
        {
            Name = user.Name,
             Mail = user.Mail,
              Address = user.Address,
               Phone = user.Phone
        });
    }
    public async override Task<GetAboutUsReply> GetAboutUs(GetAboutUsRequest request, ServerCallContext context)
    {
        bool res = false;
        string text = "";
        try
        {
            IService<AboutUs> aboutUsService = kernel.Get<IService<AboutUs>>();
            var userList = await aboutUsService.GetAll();
            var aboutUs = userList.FirstOrDefault();
            if(aboutUs != null)
            {
                if(request.Language == "ru-RU")
                    text = aboutUs.Description;
                else if(request.Language == "uk-UA")
                    text = aboutUs.DescriptionUA;
                //en-US
                else
                    text = aboutUs.DescriptionEN;
            }
            res = true;
        }
        catch (Exception ex)
        {
            res = false;
        }

        return await Task.FromResult(new GetAboutUsReply
        {
           Text = text
        });
    }
}

