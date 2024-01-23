import { SearchOptions, PagingHeaders } from './http.model';
import { Subcategory } from './subcategory.model';

export class Category {
    categoryId: string;
    name: string;
    code: string;
    sortOrder: number;

    subcategories: Subcategory[];

    constructor() {
        this.categoryId = "00000000-0000-0000-0000-000000000000";
        this.subcategories = [];
    }
}

export class CategorySearchOptions extends SearchOptions {
    q: string;
}

export class CategorySearchResponse {
    categories: Category[] = [];
    headers: PagingHeaders;
}
