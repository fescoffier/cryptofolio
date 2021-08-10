import { HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, from } from "rxjs";
import { ApiOptions } from "./api-options";

@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor {
  constructor(private api: ApiOptions) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return from(this.handle(req, next));
  }

  async handle(req: HttpRequest<any>, next: HttpHandler) {
    let authReq: HttpRequest<any>;
    if (req.url.startsWith(this.api.url)) {
      authReq = req.clone({
        withCredentials: true
      });
    } else {
      authReq = req;
    }

    return next.handle(authReq).toPromise();
  }
}