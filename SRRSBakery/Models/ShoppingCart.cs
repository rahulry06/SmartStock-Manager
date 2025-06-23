﻿using Microsoft.EntityFrameworkCore;

namespace SRRSBakery.Models
{
    public class ShoppingCart
    {
        private readonly AppDbContext _appDbContext;

        public string ShoppingCartId { get; set; }

        public List<ShoppingCartItem> ShoppingCartItems { get; set; }

        //constructor
        private ShoppingCart(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        //static method
        public static ShoppingCart GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;

            var context = services.GetService<AppDbContext>();

            string cartId = session.GetString("CartId") ?? Guid.NewGuid().ToString();
           
            var userContext = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.User;

            var user = userContext.Identity.Name;

            session.SetString("admin", "0");
            if (user == "admin@gmail.com")
            {
                session.SetString("admin", "1");
            }

            session.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }




        //instance methods
        public void AddToCart(Item item, int amount)
        {
            var shoppingCartItem =
                    _appDbContext.ShoppingCartItems.SingleOrDefault(
                        s => s.Item.ItemId == item.ItemId && s.ShoppingCartId == ShoppingCartId);
            //if it is the first time they are adding apple pie, then shoppingCartItem should be null
            if (shoppingCartItem == null)
            {
                //shoppingcartitem object and add it to customer bag
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    Item = item,
                    Amount = 1
                };

                _appDbContext.ShoppingCartItems.Add(shoppingCartItem);
            }
            else
            {
                shoppingCartItem.Amount++;
            }
            _appDbContext.SaveChanges();
        }




        public int RemoveFromCart(Item item)
        {
            var shoppingCartItem =
                    _appDbContext.ShoppingCartItems.SingleOrDefault(
                        s => s.Item.ItemId == item.ItemId && s.ShoppingCartId == ShoppingCartId);

            var localAmount = 0;

            if (shoppingCartItem != null)
            {
                //if they have more than 1 apple pie, you should reduce the count
                if (shoppingCartItem.Amount > 1)
                {
                    shoppingCartItem.Amount--;
                    localAmount = shoppingCartItem.Amount;
                }
                else
                {
                    _appDbContext.ShoppingCartItems.Remove(shoppingCartItem);
                }
            }

            _appDbContext.SaveChanges();

            return localAmount;
        }



        public int RemoveItemFromCart(Item item)
        {
            var shoppingCartItem =
                    _appDbContext.ShoppingCartItems.SingleOrDefault(
                        s => s.Item.ItemId == item.ItemId && s.ShoppingCartId == ShoppingCartId);


            var localAmount = 0;

            if (shoppingCartItem != null)
            {
                _appDbContext.ShoppingCartItems.Remove(shoppingCartItem);
            }
            _appDbContext.SaveChanges();
            return localAmount;
        }


        public List<ShoppingCartItem> GetShoppingCartItems()
        {
            return ShoppingCartItems ??
                   (ShoppingCartItems =
                       _appDbContext.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId)
                           .Include(s => s.Item)
                           .ToList());
        }



        public void ClearCart()
        {
            var cartItems = _appDbContext
                .ShoppingCartItems
                .Where(cart => cart.ShoppingCartId == ShoppingCartId);

            _appDbContext.ShoppingCartItems.RemoveRange(cartItems);

            _appDbContext.SaveChanges();
        }


        public decimal GetShoppingCartTotal()
        {
            var total = _appDbContext.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId)
                .Select(c => c.Item.Price * c.Amount).Sum();
            return total;
        }



    }
}
