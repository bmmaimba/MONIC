import { SearchOptions, PagingHeaders } from './http.model';
import { EntityType } from './entitytype.model';
import { Organisation } from './organisation.model';
import { EntityLink } from './entitylink.model';
import { Datum } from './datum.model';
import { EntityPermission } from './entitypermission.model';
import { Response } from './response.model';
import { User } from './user.model';
import { IHasFields } from './ihasfields.model';
import { FieldValue } from './fieldvalue.model';
import { OptionValue } from './optionvalue.model';
import { Roles } from './enums.model';

export class Entity implements IHasFields {
    entityId: string;
    name: string;
    code: string;
    entityTypeId: string;
    organisationId: string;
    shortName: string;
    disabled: boolean;
    entityType: EntityType;
    organisation: Organisation;

    parentEntities: EntityLink[];
    childEntities: EntityLink[];
    data: Datum[];
    entityPermissions: EntityPermission[];
    responses: Response[];
    affiliatedUsers: User[];
    fieldValues: FieldValue[] = [];
    optionValues: OptionValue[] = [];

    constructor() {
        this.entityId = "00000000-0000-0000-0000-000000000000";
    }
}

export class EntitySearchOptions extends SearchOptions {
    q: string;
    entityTypeId: string;
    organisationId: string;
    disabled: boolean;
    permission: string;
    role: Roles;
    parentEntityId: string;
    isParent: boolean;
    entityIds: string[];
}

export class EntitySearchResponse {
    entities: Entity[] = [];
    headers: PagingHeaders;
}
