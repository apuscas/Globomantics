using Exchange;
using Exchange.Interfaces;
using Globomantics.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System.Threading.Tasks;

namespace Globomantics.Controllers
{
    public class ConferenceController : Controller
    {
        private readonly IConferenceService conferenceService;
        private readonly IExchangeServiceClient exchangeServiceClient;
        private MailboxPolling mailboxPolling;

        public ConferenceController(IConferenceService conferenceService
            , IExchangeServiceClient exchangeServiceClient)
        {
            this.conferenceService = conferenceService;
            this.exchangeServiceClient = exchangeServiceClient;
            mailboxPolling = new MailboxPolling(exchangeServiceClient);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Conference";
            return View(await conferenceService.GetAll());
        }

        public IActionResult Add()
        {
            ViewBag.Title = "Add Coonference";
            return View(new ConferenceModel());
        }

        //[BasicAuthorization]
        public IActionResult StopMailboxPolling()
        {
            ViewBag.Title = "Connect To Mailbox";

            mailboxPolling.StopAsync(new System.Threading.CancellationToken());

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Add(ConferenceModel model)
        {
            if (ModelState.IsValid)
                await conferenceService.Add(model);

            return RedirectToAction("Index");
        }
    }
}
