import { Component, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { UserTestSearchOptions, UserTestSearchResponse, UserTest } from '../common/models/usertest.model';
import { UserTestService } from '../common/services/usertest.service';
import { PagingHeaders } from '../common/models/http.model';
import { ErrorService } from '../common/services/error.service';
import { CdkDragDrop, CdkDragStart, moveItemInArray } from '@angular/cdk/drag-drop';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'user-test-sort',
    templateUrl: './usertest.sort.component.html'
})
export class UserTestSortComponent implements OnInit {

    public headers: PagingHeaders = new PagingHeaders();
    public userTests: UserTest[];
    public userId: string;
    private bodyElement: HTMLElement = document.body;

    constructor(
        public modal: NgbActiveModal,
        private userTestService: UserTestService,
        private errorService: ErrorService,
        private toastr: ToastrService
    ) {
    }

    ngOnInit(): void {
        this.userTestService.search({ userId: this.userId, pageSize: 0, includeParents: true } as UserTestSearchOptions)
            .subscribe(
                response => this.userTests = response.userTests,
                err => this.errorService.handleError(err, "UserTests", "Load")
            );
    }

    dragStart(event: CdkDragStart) {
        this.bodyElement.classList.add('inheritCursors');
        this.bodyElement.style.cursor = 'grabbing';
    }

    drop(event: CdkDragDrop<UserTest[]>) {
        this.bodyElement.classList.remove('inheritCursors');
        this.bodyElement.style.cursor = 'unset';
        moveItemInArray(this.userTests, event.previousIndex, event.currentIndex);
    }

    close() {
        this.userTestService.sort(this.userId, this.userTests.map(o => o.userTestId)).subscribe(
            () => {
                this.modal.close();
                this.toastr.success("The sort order has been updated", "Change Sort Order");
            },
            err => {
                this.errorService.handleError(err, "UserTests", "Sort");
            });
    }

}

