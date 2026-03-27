import { AfterViewInit, Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[drtAutoExpandTextArea]',
  standalone: true  
})
export class AutoExpandTextAreaDirective implements AfterViewInit {
  @Input() maxRows = 3;
  private lineHeight = 24;
  private minHeight = 0;

  constructor(private el: ElementRef<HTMLTextAreaElement>) { }

  ngAfterViewInit(): void {
    const rows = this.el.nativeElement.rows || 1;
    this.minHeight = this.lineHeight * rows;
    this.resize();
  }

  @HostListener('input')
  onInput(): void {
    this.resize();
  }

  private resize(): void {
    const textarea = this.el.nativeElement;
    textarea.style.height = 'auto';

    const maxHeight = this.lineHeight * this.maxRows;
    const scrollHeight = Math.max(textarea.scrollHeight, this.minHeight);

    if (scrollHeight <= maxHeight) {
      textarea.style.overflowY = 'hidden';
      textarea.style.height = `${scrollHeight}px`;
    } else {
      textarea.style.overflowY = 'auto';
      textarea.style.height = `${maxHeight}px`;
    }
  }
}
