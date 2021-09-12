import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { CommonModule } from "@angular/common";
import { HTTP_INTERCEPTORS } from "@angular/common/http";
import { ToastrModule } from "ngx-toastr";

import { AppComponent } from "./app.component";
import { AdminLayoutComponent } from "./layouts/admin-layout/admin-layout.component";

import { AppRoutingModule } from "./app-routing.module";
import { ComponentsModule } from "./components/components.module";

import { ApiOptions } from "./api-options";
import { AuthenticationInterceptor } from "./authentication.interceptor";

@NgModule({
  declarations: [
    AppComponent,
    AdminLayoutComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    BrowserAnimationsModule,
    RouterModule,
    AppRoutingModule,
    ToastrModule.forRoot(),
    ComponentsModule
  ],
  providers: [
    ApiOptions,
    { provide: HTTP_INTERCEPTORS, useClass: AuthenticationInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
