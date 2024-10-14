import { SearchOptions, PagingHeaders } from './http.model';
import { Category } from './category.model';
import { Indicator } from './indicator.model';

export class Subcategory {
    subcategoryId: string;
    categoryId: string;
    code: string;
    name: string;
    dataEntrySubtotal: boolean;
    sortOrder: number;
    category: Category;

    indicators: Indicator[];

    constructor() {
        this.subcategoryId = "00000000-0000-0000-0000-000000000000";
        this.indicators = [];
    }
}

export class SubcategorySearchOptions extends SearchOptions {
    q: string;
    categoryId: string;
}

export class SubcategorySearchResponse {
    subcategories: Subcategory[] = [];
    headers: PagingHeaders;
}
