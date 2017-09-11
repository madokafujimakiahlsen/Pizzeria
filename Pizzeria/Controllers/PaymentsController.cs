﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Pizzeria.Services;
using Pizzeria.Models;
using Pizzeria.Models.ManageViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pizzeria.Data;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Pizzeria.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly UrlEncoder _urlEncoder;
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;

        private const string AuthenicatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public PaymentsController(
            ApplicationDbContext context,
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IEmailSender emailSender,
          ILogger<ManageController> logger,
          UrlEncoder urlEncoder,
            CartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _urlEncoder = urlEncoder;
            _cartService = cartService;
            _context = context;
        }

        public string StatusMessage { get; set; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["CardId"] = new SelectList(_context.Cards, "CardId", "Name");

            var user = await _userManager.GetUserAsync(User);
            var model = new PaymentViewModel();
            if (user == null)
            {
                return View(model);
            }
            else
            {
                model = new PaymentViewModel
                {
                    
                    PhoneNumber = user.PhoneNumber,
                    StatusMessage = StatusMessage,
                    CustomerName = user.CustomerName,
                    Email= user.Email,
                    Street = user.Street,
                    PostalCode = user.PostalCode,
                    City = user.City
                };
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PaymentViewModel model)
        {
            ViewData["CardId"] = new SelectList(_context.Cards, "CardId", "Name");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                user = new ApplicationUser();
                user.CustomerName = model.CustomerName;
                user.PhoneNumber = model.PhoneNumber;
                user.Email = model.Email;
                user.Street = model.Street;
                user.PostalCode = model.PostalCode;
                user.City = model.City;
                user.CreditCardNumber = model.CreditCardNumber;
                user.NameOnCard = model.NameOnCard;
                user.YYMM = model.YYMM;
                user.CCV = model.CCV;
                user.CardId = model.CardId;
                //throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            else
            {
                var phoneNumber = user.PhoneNumber;
                if (model.PhoneNumber != phoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                    }
                }
                user.CustomerName = model.CustomerName;
                //var setCustomerNameResult = await _userManager.UpdateAsync(user);

                user.Email = model.Email;
                user.Street = model.Street;
                //var setStreetResult = await _userManager.UpdateAsync(user);

                user.PostalCode = model.PostalCode;
                //var setPostalCodeResult = await _userManager.UpdateAsync(user);

                user.City = model.City;
                //var setCityResult = await _userManager.UpdateAsync(user);

                user.CreditCardNumber = model.CreditCardNumber;
                user.NameOnCard = model.NameOnCard;
                user.YYMM = model.YYMM;
                user.CCV = model.CCV;
                user.CardId = model.CardId;
            }
            return RedirectToAction("ComfirmPayment", user);
            //StatusMessage = "Your profile has been updated";
            //return RedirectToAction(nameof(Index));
        }

        public IActionResult ComfirmPayment(PaymentViewModel User)
        {
            User.Card = _context.Cards.Where(x => x.CardId == User.CardId).FirstOrDefault();
            return View(User);
        }

        public IActionResult Pay(IFormCollection Form,
            string customerName,string emailAddress, string phoneNumber, string street, string postalCode, string city,
            int cardId, string creditCardNumber, string nameOnCard, string yymm, string ccv)
        {
            var user = new ApplicationUser();
            user.CustomerName = customerName;
            user.PhoneNumber = phoneNumber;
            user.Email = emailAddress;
            user.Street = street;
            user.PostalCode = postalCode;
            user.City = city;
            user.CreditCardNumber = creditCardNumber;
            user.NameOnCard = nameOnCard;
            user.YYMM =yymm;
            user.CCV = ccv;
            user.CardId = cardId;

            _logger.LogCritical($"To: {user.Email}, Subject: Confirmation, Message: Thank you for your order!");

            //var key = Form.Keys.FirstOrDefault(k => k.Contains("-"));
            //    var dashPos = key.IndexOf("-");
            //    var action = key.Substring(0, dashPos);
            //    var id = int.Parse(key.Substring(dashPos + 1));
            //switch (action)
            //{
            //    case "back":
            //        return RedirectToAction("Index");
            //        break;
            //case "remove":
            //    _cartService.DeleteDish(id);
            //    break;
            //case "customize":
            //    return RedirectToAction("Customize", "CartItems", new { cartItemId = id });
            //    case "pay":
            //        return RedirectToAction("Index", "Payments", new { cartId = id });
            //}
            return RedirectToAction("Index");
        }
    }
}
