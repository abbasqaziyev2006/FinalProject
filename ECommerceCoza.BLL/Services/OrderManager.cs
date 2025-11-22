using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.BLL.Services
{
    public class OrderManager : CrudManager<Order, OrderViewModel, OrderCreateViewModel, OrderUpdateViewModel>,
        IOrderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAddressService _addressService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly BasketManager _basketManager;
        private readonly IDiscountCodeService _discountCodeService;


        public OrderManager(IRepository<Order> repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, IAddressService addressService, IOrderDetailService orderDetailService, BasketManager basketManager, IDiscountCodeService discountCodeService) : base(repository, mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _addressService = addressService;
            _orderDetailService = orderDetailService;
            _basketManager = basketManager;
            _discountCodeService = discountCodeService;
        }

        public async Task<OrderCreateViewModel> GetUserAndAddressViewModel(OrderCreateViewModel model)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser != null && currentUser.Identity!.IsAuthenticated)
            {
                var user = await _userManager.FindByNameAsync(currentUser.Identity.Name!);

                if (user != null)
                {
                    model.AppUserId = user.Id;
                    model.Email = user.Email!;

                    var addressViewModel = await _addressService.GetAsync(predicate:
                         x => x.AppUserId == user.Id && x.IsDefault && !x.IsDeleted);

                    if (addressViewModel != null)
                    {
                        //model.AddressViewModel= addressViewModel;
                        model.AddressCreateViewModel = new AddressCreateViewModel()
                        {
                            Adress = addressViewModel.Adress!,
                            FirstName = addressViewModel.FirstName!,
                            LastName = addressViewModel.LastName!,
                            Country = addressViewModel.Country!,
                            Company = addressViewModel.Company,
                            City = addressViewModel.City!,
                            Phone = addressViewModel.Phone!,
                            PostalCode = addressViewModel.PostalCode!
                        };
                    }
                }
            }

            return model;
        }

        public override async Task CreateAsync(OrderCreateViewModel model)
        {
            model.OrderDetails = await _orderDetailService.GetOrderDetailCreateViewModels();
            model.OrderStatus = OrderStatus.OnHold;

            var order = Mapper.Map<Order>(model);

            var currentUser = _httpContextAccessor.HttpContext?.User;

            order.DiscountCodeId = model.DiscountCodeId;

            if (currentUser != null && currentUser.Identity!.IsAuthenticated)
            {
                var user = await _userManager.FindByNameAsync(currentUser.Identity.Name!);

                if (user != null)
                {
                    order.AppUserId = user.Id;
                    order.Email = user.Email!;

                    var addressViewModel = await _addressService.GetAsync(predicate:
                         x => x.AppUserId == user.Id && x.IsDefault && !x.IsDeleted);

                    if (addressViewModel != null)
                        order.AddressId = addressViewModel.Id;

                }
            }
            else
            {
                if (model.AddressCreateViewModel != null)
                {
                    var address = await _addressService.CreateAddressAsync(model.AddressCreateViewModel);
                    order.AddressId = address.Id;
                }
            }

            await Repository.CreateAsync(order);
        }

        public async Task<DiscountCodeViewModel> GetDiscount(string discountCode)
        {
            var basket = await _basketManager.GetBasketAsync();

            var discount = await _discountCodeService.GetAsync(predicate:
                    x => x.Code == discountCode && x.IsActive && !x.IsDeleted);

            return discount!;
        }

        public async Task<List<OrderViewModel>> GetOrderViewModelsAsync(string userId)
        {
            var model = await GetAllAsync(predicate: x => x.AppUser!.Id == userId && !x.IsDeleted,
                include: x => x.Include(od => od.OrderDetails).ThenInclude(pv => pv.ProductVariant).ThenInclude(c => c.Color!)
                .Include(p => p.OrderDetails).ThenInclude(p => p.ProductVariant).ThenInclude(p => p.Product!));

            return model.ToList();
        }

        public async Task<OrderViewModel> GetDetailsOfOrderAsync(int orderId)
        {
            var order = await GetAsync(predicate: x => x.Id == orderId && !x.IsDeleted,
                include: x => x.Include(od => od.OrderDetails).ThenInclude(pv => pv.ProductVariant).ThenInclude(p => p.Product!)
                .Include(od => od.OrderDetails).ThenInclude(pv => pv.ProductVariant).ThenInclude(c => c.Color!)
                .Include(a => a.Address));


            if (order == null)
                return null!;


            foreach (var detail in order.OrderDetails)
            {
                detail.TotalPrice = detail.ProductVariant!.Priced * detail.Quantity;
            }
            return order;
        }

        //public async Task<OrderUpdateViewModel> GetOrderUpdateViewModelAsync(int id)
        //{
        //    var order = await Repository.GetAsync(predicate: x => x.Id == id,
        //        include: x => x.Include(o => o.OrderDetails).ThenInclude(p => p.ProductVariant)
        //        .Include(a => a.Address).Include(u => u.AppUser!));

        //    if (order == null)
        //        return null!;
        //    var model = Mapper.Map<OrderUpdateViewModel>(order);

        //    return model;
        //} 
    }
}
