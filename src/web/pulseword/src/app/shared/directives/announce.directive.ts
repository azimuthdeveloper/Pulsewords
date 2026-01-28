import { Directive, ElementRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { LiveAnnouncer } from '@angular/cdk/a11y';

@Directive({
  selector: '[appAnnounce]',
  standalone: true
})
export class AnnounceDirective implements OnChanges {
  @Input('appAnnounce') message: string = '';
  @Input() announcePoliteness: 'polite' | 'assertive' = 'polite';

  constructor(private liveAnnouncer: LiveAnnouncer) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['message'] && this.message) {
      this.liveAnnouncer.announce(this.message, this.announcePoliteness);
    }
  }
}
