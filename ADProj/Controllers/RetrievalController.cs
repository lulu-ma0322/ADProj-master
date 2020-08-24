﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ADProj.Services;
using ADProj.Models;
using Microsoft.AspNetCore.Http;

namespace ADProj.Controllers
{
    public class RetrievalController : Controller
    {
        private RetrievalService rs;
        private InventoryService invsvc;

        public RetrievalController(RetrievalService rs, InventoryService invsvc)
        {
            this.rs = rs;
            this.invsvc = invsvc;
        }
        public IActionResult Index()
        {
            List<Retrieval> retrivals = rs.GetRetrievals();
            ViewData["rtList"] = retrivals;
            return View();
        }

        public IActionResult GenerateRetrievalList()
        {
            Dictionary<string, int> retrieveList = new Dictionary<string, int>();
            List<Request> requests = rs.RetrieveApprovedRequest();
            int empId = Convert.ToInt32(HttpContext.Session.GetString("id"));
            foreach (Request r in requests)
            {
                List<RequestDetails> rds = rs.FindRquestDetailsById(r.Id);
                foreach (RequestDetails rd in rds)
                {
                    if (retrieveList.ContainsKey(rd.InventoryItemId))
                    {
                        int currqty = retrieveList[rd.InventoryItemId];
                        int updateqty = currqty + rd.QtyRequested;
                        retrieveList[rd.InventoryItemId] = updateqty;
                    }
                    else
                    {
                        retrieveList[rd.InventoryItemId] = rd.QtyRequested;
                    }
                }
            }
            Retrieval ret = rs.CreateRetrieval(empId, requests);
            rs.CreateRetrievalDetails(ret.Id, retrieveList);
            List<RetrievalDetails> rtList = rs.FindRetrievalDetails(ret.Id);
            invsvc.ResetRequestQtyByRetrievalId(ret.Id);
            ViewData["retrieveList"] = rtList;
            ViewData["retId"] = ret.Id;
            return View();
        }

        public IActionResult RetrievalDetails(int rId)
        {
            List<RetrievalDetails> rtd = rs.FindRetrievalDetails(rId);
            Request req = rs.FindRequestByRetId(rId);
            Retrieval ret = rs.FindRetById(rId);
            ViewData["disbId"] = req.DisbursementId;
            ViewData["rtd"] = rtd;
            ViewData["ret"] = ret;
            return View();
        }
    }
}