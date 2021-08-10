import { Component, OnInit, Input, ViewChild, ElementRef } from "@angular/core";

@Component({
  selector: "app-picture-upload",
  templateUrl: "./picture-upload.component.html",
  styleUrls: ["./picture-upload.component.css"]
})
export class PictureUploadComponent implements OnInit {
  @Input() avatar: boolean = false;
  @Input() image: string;
  file: any = {};
  imagePreviewUrl: any = {};
  @ViewChild("fileInput") fileInput: ElementRef;
  constructor() {
    this.handleImageChange = this.handleImageChange.bind(this);
  }

  ngOnInit() {
    this.file = null;
    this.imagePreviewUrl =
      this.image !== undefined
        ? this.image
        : this.avatar
        ? "assets/img/placeholder.jpg"
        : "assets/img/image_placeholder.jpg";
  }
  handleImageChange($event) {
    $event.preventDefault();
    let reader = new FileReader();
    let file = $event.target.files[0];
    reader.onloadend = () => {
      this.file = file;
      this.imagePreviewUrl = reader.result;
      // this.state.imagePreviewUrl1 = reader.result;
    };
    reader.readAsDataURL(file);
  }
  handleClick() {
    console.log(this.fileInput.nativeElement);
    this.fileInput.nativeElement.click();
  }
  handleRemove() {
    this.file = null;
    this.imagePreviewUrl =
      this.image !== undefined
        ? this.image
        : this.avatar
        ? "assets/img/placeholder.jpg"
        : "assets/img/image_placeholder.jpg";
    this.fileInput.nativeElement.value = null;
  }
  handleSubmit($event) {
    $event.preventDefault();
    // this.state.file is the file/image uploaded
    // in this function you can save the image (this.state.file) on form submit
    // you have to call it yourself
  }
}
