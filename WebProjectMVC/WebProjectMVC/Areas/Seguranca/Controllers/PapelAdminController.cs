﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebProjectMVC.Areas.Seguranca.Models;
using WebProjectMVC.Infraestrutura;

namespace WebProjectMVC.Areas.Seguranca.Controllers
{
    [Authorize(Roles = "Administradores")]
    public class PapelAdminController : Controller
    {
        // GET: Seguranca/RoleAdmin
        public ActionResult Index()
        {
            return View(RoleManager.Roles);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create([Required]string nome)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = RoleManager.Create(new Papel(nome));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(nome);
        }

        public ActionResult Edit(string id)
        {
            Papel papel = RoleManager.FindById(id);
            string[] memberIDs = papel.Users.Select(x => x.UserId).ToArray();
            IEnumerable<Usuario> membros = UserManager.Users.Where(x => memberIDs.Any(y => y == x.Id));
            IEnumerable<Usuario> naoMembros = UserManager.Users.Except(membros);
            return View(new PapelEditModel
            {
                Role = papel,
                Membros = membros,
                NaoMembros = naoMembros
            });
        }

        [HttpPost]
        public ActionResult Edit(PapelModificationModel model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.IdsParaAdicionar ?? new string[] { })
                {
                    result = UserManager.AddToRole(userId, model.NomePapel);
                    if (!result.Succeeded)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }

                foreach (string userId in model.IdsParaRemover ?? new string[] { })
                {
                    result = UserManager.RemoveFromRole(userId, model.NomePapel);
                    if (!result.Succeeded)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }

                return RedirectToAction("Index");
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        private GerenciadorPapel RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<GerenciadorPapel>();
            }
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private GerenciadorUsuario UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<GerenciadorUsuario>();
            }
        }
    }
}