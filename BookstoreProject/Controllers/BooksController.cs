﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookstoreProject.Data;
using BookstoreProject.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;

namespace BookstoreProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnviroment;
        private readonly IStringLocalizer<BooksController> _localizer;
        public BooksController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, IStringLocalizer<BooksController> localizer)
        {
            _context = context;
            _hostEnviroment = hostEnvironment;
            _localizer = localizer;
        }


        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Books.Include(b => b.Author).Include(b => b.Category).Include(b => b.Language);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Language)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => a.Active == true), "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(a => a.Active == true), "Id", "Name");
            ViewData["LanguageId"] = new SelectList(_context.Languages.Where(a => a.Active == true), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                if(book.MainPhotoFile == null || book.SecondPhotoFile == null || book.ThirdPhotoFile == null)
                {
                    ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
                    ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Name", book.LanguageId);
                    ViewData["ErrorMessage"] = _localizer["ErrorMessage1"];
                    return View();
                }

                string[] imgext = new string[3];

                imgext[0] = Path.GetExtension(book.MainPhotoFile.FileName);
                imgext[1] = Path.GetExtension(book.SecondPhotoFile.FileName);
                imgext[2] = Path.GetExtension(book.ThirdPhotoFile.FileName);

                if ((imgext[0] == ".jpg" || imgext[0] == ".png") && (imgext[1] == ".jpg" || imgext[1] == ".png") && (imgext[2] == ".jpg" || imgext[2] == ".png"))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        string path_name = Guid.NewGuid().ToString() + imgext[i];
                        string saveimg = Path.Combine(_hostEnviroment.WebRootPath, "images", path_name);

                        switch (i)
                        {
                            case 0:
                                book.MainPhoto = path_name;
                                using (var uploadimg = new FileStream(saveimg, FileMode.Create))
                                {
                                    await book.MainPhotoFile.CopyToAsync(uploadimg);
                                }
                                break;
                            case 1:
                                book.SecondPhoto = path_name;
                                using (var uploadimg = new FileStream(saveimg, FileMode.Create))
                                {
                                    await book.SecondPhotoFile.CopyToAsync(uploadimg);
                                }
                                break;
                            case 2:
                                book.ThirdPhoto = path_name;
                                using (var uploadimg = new FileStream(saveimg, FileMode.Create))
                                {
                                    await book.ThirdPhotoFile.CopyToAsync(uploadimg);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }


                book.CreateDate = DateTime.Now;
                book.Active = true;
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Name", book.LanguageId);
            return View(book);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            
            //List<SelectList> degerler = _context.Authors.Where(a => a.Active == true).ToList();
            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a=>a.Active==true), "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(a => a.Active == true), "Id", "Name", book.CategoryId);
            ViewData["LanguageId"] = new SelectList(_context.Languages.Where(a => a.Active == true), "Id", "Name", book.LanguageId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string[] imgext = new string[3];

                imgext[0] = book.MainPhotoFile == null ? String.Empty : Path.GetExtension(book.MainPhotoFile.FileName);
                imgext[1] = book.SecondPhotoFile == null ? String.Empty : Path.GetExtension(book.SecondPhotoFile.FileName);
                imgext[2] = book.ThirdPhotoFile == null ? String.Empty : Path.GetExtension(book.ThirdPhotoFile.FileName);

                if ((imgext[0] == ".jpg" || imgext[0] == ".png" || imgext[0] == String.Empty) && (imgext[1] == ".jpg" || imgext[1] == ".png" || imgext[1] == String.Empty) && (imgext[2] == ".jpg" || imgext[2] == ".png" || imgext[2] == String.Empty))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if(imgext[i] != String.Empty)
                        {
                            string path_name = Guid.NewGuid().ToString() + imgext[i];
                            string saveimg = Path.Combine(_hostEnviroment.WebRootPath, "images", path_name);

                            switch (i)
                            {
                                case 0:
                                    book.MainPhoto = path_name;
                                    using (var uploadimg = new FileStream(saveimg, FileMode.Create))
                                    {
                                        await book.MainPhotoFile.CopyToAsync(uploadimg);
                                    }
                                    break;
                                case 1:
                                    book.SecondPhoto = path_name;
                                    using (var uploadimg = new FileStream(saveimg, FileMode.Create))
                                    {
                                        await book.SecondPhotoFile.CopyToAsync(uploadimg);
                                    }
                                    break;
                                case 2:
                                    book.ThirdPhoto = path_name;
                                    using (var uploadimg = new FileStream(saveimg, FileMode.Create))
                                    {
                                        await book.ThirdPhotoFile.CopyToAsync(uploadimg);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        } 
                    } 
                } else
                {
                    ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => a.Active == true), "Id", "Name", book.AuthorId);
                    ViewData["CategoryId"] = new SelectList(_context.Categories.Where(a => a.Active == true), "Id", "Name", book.CategoryId);
                    ViewData["LanguageId"] = new SelectList(_context.Languages.Where(a => a.Active == true), "Id", "Name", book.LanguageId);
                    ViewData["ErrorMessage"] = _localizer["ErrorMessage2"];
                    return View();
                }

                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Name", book.LanguageId);
            return View(book);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Language)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Book book = _context.Books.Where(x => x.Id == id).FirstOrDefault();

            if (book != null)
            {
                if(book.Active==true)
                {
                    book.Active = false;
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    book.Active = true;
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                
            }
            else
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));



        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
