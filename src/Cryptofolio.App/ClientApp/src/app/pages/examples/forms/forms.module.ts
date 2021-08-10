import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { TagInputModule } from "ngx-chips";
import { JwBootstrapSwitchNg2Module } from "jw-bootstrap-switch-ng2";
import { AngularMultiSelectModule } from "angular2-multiselect-dropdown";
// import { ArchwizardModule } from "angular-archwizard";
import { ModalModule } from "ngx-bootstrap/modal";
import { BsDatepickerModule } from "ngx-bootstrap/datepicker";
import { TimepickerModule } from "ngx-bootstrap/timepicker";
import { ProgressbarModule } from "ngx-bootstrap/progressbar";
import { BsDropdownModule } from "ngx-bootstrap/dropdown";
import { ComponentsModule } from "../../../components/components.module";
import { TabsModule } from "ngx-bootstrap/tabs";

import { CommonModule } from "@angular/common";
import { RegularComponent } from "./regular/regular.component";
import { ExtendedComponent } from "./extended/extended.component";
import { ValidationComponent } from "./validation/validation.component";
import { WizardComponent } from "./wizard/wizard.component";
import { FormsRoutes } from "./forms.routing";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule.withConfig({ warnOnNgModelWithFormControl: "never" }),
    RouterModule.forChild(FormsRoutes),
    TagInputModule,
    AngularMultiSelectModule,
    // ArchwizardModule,
    ModalModule.forRoot(),
    BsDatepickerModule.forRoot(),
    ComponentsModule,
    TimepickerModule.forRoot(),
    BsDropdownModule.forRoot(),
    TabsModule.forRoot(),
    ProgressbarModule.forRoot(),
    JwBootstrapSwitchNg2Module
  ],
  declarations: [
    RegularComponent,
    ExtendedComponent,
    ValidationComponent,
    WizardComponent
  ]
})
export class Forms {}
