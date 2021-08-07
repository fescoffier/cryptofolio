import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

import { BsDropdownModule } from "ngx-bootstrap/dropdown";
import { ProgressbarModule } from "ngx-bootstrap/progressbar";
import { TooltipModule } from "ngx-bootstrap/tooltip";
import { PaginationModule } from "ngx-bootstrap/pagination";
import { CollapseModule } from "ngx-bootstrap/collapse";
import { AlertModule } from "ngx-bootstrap/alert";
import { ModalModule } from "ngx-bootstrap/modal";
import { TabsModule } from "ngx-bootstrap/tabs";

import { ButtonsComponent } from "./buttons/buttons.component";
import { PanelsComponent } from "./panels/panels.component";
import { GridComponent } from "./grid/grid.component";
import { SweetalertComponent } from "./sweetalert/sweetalert.component";
import { NotificationsComponent } from "./notifications/notifications.component";
import { IconsComponent } from "./icons/icons.component";
import { TypographyComponent } from "./typography/typography.component";

import { ComponentsRoutes } from "./components.routing";

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(ComponentsRoutes),
    FormsModule,
    BsDropdownModule.forRoot(),
    ProgressbarModule.forRoot(),
    TooltipModule.forRoot(),
    PaginationModule.forRoot(),
    CollapseModule.forRoot(),
    TabsModule.forRoot(),
    AlertModule.forRoot(),
    ModalModule.forRoot()
  ],
  declarations: [
    ButtonsComponent,
    PanelsComponent,
    GridComponent,
    SweetalertComponent,
    NotificationsComponent,
    IconsComponent,
    TypographyComponent
  ],
  exports: [
    ButtonsComponent,
    PanelsComponent,
    GridComponent,
    SweetalertComponent,
    NotificationsComponent,
    IconsComponent,
    TypographyComponent
  ]
})
export class ComponentsPageModule {}
