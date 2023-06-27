import { SearchOptions, PagingHeaders } from './http.model';
import { DataReview } from './datareview.model';
import { Date as LocalDate } from './date.model';
import { Entity } from './entity.model';
import { Indicator } from './indicator.model';
import { User } from './user.model';
import { Category } from './category.model';
import { Subcategory } from './subcategory.model';
import { DataReviewLink } from './datareviewlink.model';
import { DateTypes } from './enums.model';

export class Datum {
    indicatorId: string;
    entityId: string;
    dateId: string;
    value: number;
    note: string;
    submitted: boolean;
    submitDataReviewId: string;
    verified: boolean;
    verifyDataReviewId: string;
    approved: boolean;
    approveDataReviewId: string;
    rejectDataReviewId: string;
    rejected: boolean;
    approveReview: DataReview;
    rejectReview: DataReview;
    submitReview: DataReview;
    verifyReview: DataReview;
    date: LocalDate;
    entity: Entity;
    indicator: Indicator;

    dataReviewLinks: DataReviewLink[];

    constructor() {
    }
}

export class DatumSearchOptions extends SearchOptions {
    indicatorId: string;
    entityId: string;
    dateId: string;
    aggregated: boolean;
    submitDataReviewId: string;
    verifyDataReviewId: string;
    approveDataReviewId: string;
    rejectDataReviewId: string;
    lastSavedById: string;
    dateType: DateTypes;
}

export class DatumSearchResponse {
    data: Datum[] = [];
    headers: PagingHeaders;
}

export class MyDataSearchOptions extends SearchOptions {
     data: Datum[] = [];
     headers: PagingHeaders;
     q: string;
     indicatorId: string;
     entityId: string;
     categoryId: string;
     subcategoryId: string;
     dateId: string;
     canEdit: boolean;
     submitted: boolean;
     canSubmit: boolean;
     approved: boolean;
     canApprove: boolean;
}

export class DataStatus {
    missing: number;
    captured: number;
    submitted: number;
    verified: number;
    approved: number;
}

export class DataStatusOptions {
    indicatorIds: string[];
    entityIds: string[];
    dateIds: string[];
}

export class DataEntryDatum {
    indicatorId: string;
    entityId: string;
    dateId: string;
    value: number;
    note: string;
}
