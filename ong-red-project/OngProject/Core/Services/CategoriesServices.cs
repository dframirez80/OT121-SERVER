﻿using OngProject.Common;
using OngProject.Core.DTOs;
using OngProject.Core.DTOs.CategoriesDTOs;
using OngProject.Core.Entities;
using OngProject.Core.Helper.Pagination;
using OngProject.Core.Interfaces.IServices;
using OngProject.Core.Interfaces.IServices.AWS;
using OngProject.Core.Mapper;
using OngProject.Infrastructure.Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OngProject.Core.Services
{
    public class CategoriesServices : ICategoriesServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageServices;
        private readonly IUriService _uriService;
        private readonly EntityMapper _entityMapper;

        public CategoriesServices(IUnitOfWork unitOfWork, IImageService imageServices, IUriService uriService)
        {
            _unitOfWork = unitOfWork;
            _imageServices = imageServices;
            _uriService = uriService;
            _entityMapper = new EntityMapper();
        }

        public bool EntityExist(int id)
        {
            return _unitOfWork.CategoryRepository.EntityExists(id);
        }

        public async Task<Result> Delete(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetById(id);
            if (category == null)
            {
                return new Result().NotFound();
            }
            var ulr = category.Image;
            var result = await _unitOfWork.CategoryRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
            if (result != null)
            {
                //await _imageServices.Delete(ulr);
                return new Result().Success("Category eliminado con exito");
            }
            return new Result().Fail("Ocurrio un error al eliminar la categoria.");
        }

        public async Task<PaginationDTO<string>> GetByPagingAsync(int page, int quantity)
        {
            //return (await _unitOfWork.CategoryRepository.GetAll())
            //.Select(c => c.Name).ToArray();

            var totalItems = await _unitOfWork.CategoryRepository.CountAsync();
            var totalPages = (int)Math.Floor((decimal)totalItems / page);

            if(page > totalPages)
                return null;

            var categoriesByName = (await _unitOfWork
                .CategoryRepository.GetPageAsync(c => c.Name, quantity, page))
                .Select(c => c.Name).ToList<string>();

            var pagingResponse = new PaginationDTO<string>()
            {
                CurrentPage = page,
                TotalItems = totalItems,
                TotalPages = totalItems % quantity > 0 ? totalItems+1 : totalPages,
                PrevPage = page > 1 ?_uriService.GetPage("/Category", page - 1) : String.Empty,
                NextPage = page < totalPages ? _uriService.GetPage("/Category", page + 1) : String.Empty,
                Items = categoriesByName
            };

            return pagingResponse;
        }

        public async Task<CategoryGetDTO> Get(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetById(id);
            return category != null ? _entityMapper.FromCategoriesToCategoriesDTO(category) : null;
        }

        public async Task<bool> ExistsByName(CategoryInsertDTO category){
            var exists = await _unitOfWork.CategoryRepository.FindByCondition(c => c.Name == category.Name);
            if (exists != null)
            {
                if (exists.FirstOrDefault() != null)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<CategoryGetDTO> Insert(CategoryInsertDTO category)
        {
            
            CategoryGetDTO  categoryGetDTO = null;
            string imageUrl = String.Empty,
                  imageName = category.Image != null ? category.Image.Name : String.Empty;

            try{

                if (imageName != String.Empty)
                {
                    Result res = await _imageServices.Save($"{Guid.NewGuid()}_{category.Image.FileName}", category.Image);
                    if (!res.Messages[0].StartsWith("https")){
                        return null;
                    }else{
                        imageUrl = res.Messages[0];
                    }
                }
                categoryGetDTO = _entityMapper.FromCategoryInsertDTOToCategoryGetDTO(category, imageUrl);
                Category newCategory = _entityMapper.FromCategoryGetDTOToCategory(categoryGetDTO);
                await _unitOfWork.CategoryRepository.Insert(newCategory);
                await _unitOfWork.SaveChangesAsync();
            }catch(Exception e){
                return null;
            }
            
            return categoryGetDTO;
        }

        public async Task<Category> FindById(Int32 id)
        {
            return await _unitOfWork.CategoryRepository.GetById(id);
        }

        public async Task<Category> Update(CategoryUpdateDTO categoryInfo, Int32 id)
        {
            Category category = await FindById(id);
            if (category != null)
            {

                string imageUrl = String.Empty;
                if (categoryInfo.Image != null)
                {
                    if(!String.IsNullOrEmpty(category.Image))
                    {
                        if(!await _imageServices.Delete(category.Image))
                        {
                            return null;
                        }
                    }
                        Result savedImage = await _imageServices.Save($"{Guid.NewGuid()}_{categoryInfo.Image.FileName}", categoryInfo.Image);
                        if (savedImage.HasErrors)
                        {
                            return null;
                        }
                        imageUrl = savedImage.Messages[0];

                }
                category = _entityMapper.FromCategoryUpdateDTOToCategory(categoryInfo, category, imageUrl);
                Result result = await _unitOfWork.CategoryRepository.Update(category);
                if(!result.HasErrors){
                    await _unitOfWork.SaveChangesAsync();
                }
                return result.HasErrors ? null : category;
            }
            return null;
        }
    }
}
