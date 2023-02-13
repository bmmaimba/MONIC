import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ChangePasswordModel } from '../common/models/auth.models';
import { ProfileModel } from '../common/models/profile.models';
import { AuthService } from '../common/services/auth.service';
import { ScrollSpyService } from '../common/scroll-spy/scroll-spy.service';
import { ErrorService } from '../common/services/error.service';

@Component({
    selector: 'app-account',
    templateUrl: './account.component.html'
})
export class AccountComponent implements OnInit, AfterViewInit {

    public profile: ProfileModel = {} as any;
    public changePasswordModel: ChangePasswordModel = new ChangePasswordModel();

    // todo: remove this
    public rootPath = "https://d33wubrfki0l68.cloudfront.net/053f2dfd0df2f52c41e903a21d177b0b44abc9b1/1282c";
    public activeTarget: string;

    @ViewChild('basicInformationSection', { static: true }) basicInformationSection: ElementRef;
    @ViewChild('usernameSection', { static: true }) usernameSection: ElementRef;
    @ViewChild('passwordSection', { static: true }) passwordSection: ElementRef;

    constructor(
        private toastr: ToastrService,
        private spyService: ScrollSpyService,
        private authService: AuthService,
        private errorService: ErrorService
    ) {
    }

    ngOnInit(): void {

        this.authService.getProfile().subscribe(o => this.profile = o);

        this.spyService.addTarget({ name: 'basicInformationSection', element: this.basicInformationSection });
        this.spyService.addTarget({ name: 'usernameSection', element: this.usernameSection });
        this.spyService.addTarget({ name: 'passwordSection', element: this.passwordSection });
    }

    ngAfterViewInit(): void {
        this.spyService.spy({ thresholdBottom: 250 });
    }

    saveBasicInformation(form: NgForm): void {

        if (form.invalid) {

            this.toastr.error("The form has not been completed correctly.", "Form Error");
            return;

        }

        // todo...
        //this.userService.save(this.user)
        //    .subscribe(
        //        user => {
        this.toastr.success("The user has been saved", "Save User");
        //            if (this.isNew) {
        //                this.ngOnDestroy();
        //                this.router.navigate(["../", user.id], { relativeTo: this.route });
        //            }
        //            else {
        //                // reload profile if editing self
        //                if (this.user.id === this.profile.userId)
        //                    this.authService.getProfile(true).subscribe();
        //            }
        //        },
        //        err => {
        //            this.errorService.handleError(err, "User", "Save");
        //        }
        //    );

    }

    changePassword(form: NgForm): void {

        if (form.invalid) {

            this.toastr.error("The form has not been completed correctly.", "Form Error");
            return;

        }

        if (this.changePasswordModel.newPassword !== this.changePasswordModel.confirmPassword) {

            this.toastr.error("The new and confirm passwords are different.", "Form Error");
            return;

        }

        this.authService.changePassword(this.changePasswordModel)
            .subscribe(
                () => {
                    this.toastr.success("Your password has been changed", "Change Password");
                    form.resetForm();
                    this.changePasswordModel = new ChangePasswordModel();
                },
                err => {
                    this.errorService.handleError(err, "Password", "Change");
                }
            );

    }

    onStrength(strength: any) {
        console.info( strength );
        //this.user.strength = strength;
    }

}

