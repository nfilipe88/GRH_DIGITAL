import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GestaoColaboradores } from './gestao-colaboradores';

describe('GestaoColaboradores', () => {
  let component: GestaoColaboradores;
  let fixture: ComponentFixture<GestaoColaboradores>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GestaoColaboradores]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GestaoColaboradores);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
